using MediatR;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Admin.Users.Commands.UpdateUserRole;

public class UpdateUserRoleCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUserRoleCommand, UserDto>
{
    private static readonly string[] ValidRoles = ["Admin", "Customer"];

    public async Task<UserDto> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (!ValidRoles.Contains(request.NewRole))
            throw new ConflictException($"Role không hợp lệ: {request.NewRole}");

        var user = await userRepository.GetByIdAsync(request.TargetUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.TargetUserId);

        if (request.TargetUserId == request.RequestingUserId && request.NewRole != "Admin")
            throw new ConflictException("Không thể tự hạ cấp quyền của mình.");

        user.UpdateRole(request.NewRole);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsBanned = user.IsBanned,
            BannedAt = user.BannedAt,
            CreatedAt = user.CreatedAt
        };
    }
}
