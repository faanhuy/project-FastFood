# scaffold-feature.ps1
# Generate CQRS skeleton files for a new SmartShop feature
#
# Usage:
#   .\tools\scaffold-feature.ps1 -EntityName Product
#   .\tools\scaffold-feature.ps1 -EntityName Review -Plural Reviews

param(
    [Parameter(Mandatory)]
    [string]$EntityName,

    [string]$Plural = "${EntityName}s"
)

$AppRoot = Join-Path $PSScriptRoot "..\src\SmartShop.Application\Features\$Plural"
$ApiRoot = Join-Path $PSScriptRoot "..\src\SmartShop.WebAPI\Controllers"
$AppNs   = "SmartShop.Application.Features.$Plural"
$ApiNs   = "SmartShop.WebAPI.Controllers"

function Write-File($path, $content) {
    $dir = Split-Path $path
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
    if (Test-Path $path) {
        Write-Warning "  SKIP (already exists): $path"
    } else {
        [System.IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  CREATE: $path" -ForegroundColor Green
    }
}

# --- 1. Response DTO ----------------------------------------------------------
Write-File "$AppRoot\${EntityName}Response.cs" "namespace $AppNs;

public record ${EntityName}Response(
    Guid Id
    // TODO: add return properties
);
"

# --- 2. Create Command --------------------------------------------------------
Write-File "$AppRoot\Commands\Create${EntityName}\Create${EntityName}Command.cs" "using MediatR;

namespace $AppNs.Commands.Create${EntityName};

public record Create${EntityName}Command(
    // TODO: add input fields
) : IRequest<${EntityName}Response>;
"

Write-File "$AppRoot\Commands\Create${EntityName}\Create${EntityName}CommandHandler.cs" "using MediatR;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace $AppNs.Commands.Create${EntityName};

public class Create${EntityName}CommandHandler(
    I${EntityName}Repository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<Create${EntityName}Command, ${EntityName}Response>
{
    public async Task<${EntityName}Response> Handle(Create${EntityName}Command request, CancellationToken cancellationToken)
    {
        // TODO: 1. Check duplicate if needed
        // var existing = await repository.GetBy...Async(...);
        // if (existing is not null) throw new ConflictException(""..."");

        // TODO: 2. Create entity via factory method
        // var entity = ${EntityName}.Create(...);

        // TODO: 3. Save
        // await repository.AddAsync(entity, cancellationToken);
        // await unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: 4. Map and return response
        throw new NotImplementedException();
    }
}
"

# --- 3. Delete Command --------------------------------------------------------
Write-File "$AppRoot\Commands\Delete${EntityName}\Delete${EntityName}Command.cs" "using MediatR;

namespace $AppNs.Commands.Delete${EntityName};

public record Delete${EntityName}Command(Guid Id) : IRequest;
"

Write-File "$AppRoot\Commands\Delete${EntityName}\Delete${EntityName}CommandHandler.cs" "using MediatR;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace $AppNs.Commands.Delete${EntityName};

public class Delete${EntityName}CommandHandler(
    I${EntityName}Repository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<Delete${EntityName}Command>
{
    public async Task Handle(Delete${EntityName}Command request, CancellationToken cancellationToken)
    {
        // TODO: 1. Find entity, throw NotFoundException if not found
        // var entity = await repository.GetByIdAsync(request.Id, cancellationToken)
        //     ?? throw new NotFoundException(nameof(${EntityName}), request.Id);

        // TODO: 2. Check constraints before delete if needed
        // if (...) throw new ConflictException(""..."");

        // TODO: 3. Delete and save
        // repository.Delete(entity);
        // await unitOfWork.SaveChangesAsync(cancellationToken);

        throw new NotImplementedException();
    }
}
"

# --- 4. GetAll Query ----------------------------------------------------------
Write-File "$AppRoot\Queries\GetAll${Plural}\GetAll${Plural}Query.cs" "using MediatR;

namespace $AppNs.Queries.GetAll${Plural};

public record GetAll${Plural}Query() : IRequest<IEnumerable<${EntityName}Response>>;
"

Write-File "$AppRoot\Queries\GetAll${Plural}\GetAll${Plural}QueryHandler.cs" "using MediatR;
using SmartShop.Domain.Interfaces;

namespace $AppNs.Queries.GetAll${Plural};

public class GetAll${Plural}QueryHandler(
    I${EntityName}Repository repository) : IRequestHandler<GetAll${Plural}Query, IEnumerable<${EntityName}Response>>
{
    public async Task<IEnumerable<${EntityName}Response>> Handle(
        GetAll${Plural}Query request, CancellationToken cancellationToken)
    {
        var items = await repository.GetAllAsync(cancellationToken);

        // TODO: map to ${EntityName}Response
        // return items.Select(x => new ${EntityName}Response(x.Id, ...));

        throw new NotImplementedException();
    }
}
"

# --- 5. Controller ------------------------------------------------------------
$routeName = $Plural.ToLower()

Write-File "$ApiRoot\${Plural}Controller.cs" "using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Application.Common.Models;
using $AppNs;
using $AppNs.Commands.Create${EntityName};
using $AppNs.Commands.Delete${EntityName};
using $AppNs.Queries.GetAll${Plural};

namespace $ApiNs;

[ApiController]
[Route(""api/$routeName"")]
[Authorize]
public class ${Plural}Controller(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<${EntityName}Response>>>> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAll${Plural}Query(), ct);
        return Ok(ApiResponse<IEnumerable<${EntityName}Response>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<${EntityName}Response>>> Create(
        [FromBody] Create${EntityName}Command command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), ApiResponse<${EntityName}Response>.Ok(result));
    }

    [HttpDelete(""{id:guid}"")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new Delete${EntityName}Command(id), ct);
        return Ok(ApiResponse<string>.Ok(""Deleted successfully.""));
    }
}
"

# --- Done ---------------------------------------------------------------------
Write-Host ""
Write-Host "Done! Remaining manual steps:" -ForegroundColor Cyan
Write-Host "  1. Domain entity:    src/SmartShop.Domain/Entities/${EntityName}.cs"
Write-Host "  2. EF config:        src/SmartShop.Infrastructure/Persistence/Configurations/${EntityName}Configuration.cs"
Write-Host "  3. DbSet:            ApplicationDbContext -> public DbSet<${EntityName}> ${Plural} { get; set; }"
Write-Host "  4. Repository:       src/SmartShop.Domain/Interfaces/I${EntityName}Repository.cs"
Write-Host "                       src/SmartShop.Infrastructure/Repositories/${EntityName}Repository.cs"
Write-Host "  5. DI registration:  services.AddScoped<I${EntityName}Repository, ${EntityName}Repository>();"
Write-Host "  6. Migration:        dotnet ef migrations add Add${EntityName}Schema --project src/SmartShop.Infrastructure --startup-project src/SmartShop.WebAPI"
Write-Host "  7. Fill in TODO comments in the generated handlers"
