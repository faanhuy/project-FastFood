using MediatR;
using SmartShop.Application.Features.Common;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Admin.Users.Commands.BulkUpdateUsers;

public class BulkUpdateUsersCommandHandler(
    IUserRepository repository,
    IUnitOfWork unitOfWork
) : IRequestHandler<BulkUpdateUsersCommand, BulkActionResult>
{
    public async Task<BulkActionResult> Handle(BulkUpdateUsersCommand request, CancellationToken cancellationToken)
    {
        // Max 100 items per batch
        if (request.UserIds.Count > 100)
            throw new ConflictException("error.bulk_max_items", null);

        var users = await repository.GetByIdsAsync(request.UserIds, cancellationToken);
        var errors = new List<BulkItemError>();
        var succeeded = 0;

        foreach (var id in request.UserIds)
        {
            // Prevent self-action
            if (id == request.AdminUserId)
            {
                errors.Add(new BulkItemError(id, "Cannot perform action on yourself"));
                continue;
            }

            var user = users.FirstOrDefault(u => u.Id == id);
            if (user is null)
            {
                errors.Add(new BulkItemError(id, "User not found"));
                continue;
            }

            try
            {
                switch (request.Action.ToLowerInvariant())
                {
                    case "ban":
                        user.Ban();
                        succeeded++;
                        break;
                    case "unban":
                        user.Unban();
                        succeeded++;
                        break;
                    case "setrole":
                        if (string.IsNullOrWhiteSpace(request.RoleValue))
                            errors.Add(new BulkItemError(id, "RoleValue is required for setRole action"));
                        else
                        {
                            user.UpdateRole(request.RoleValue);
                            succeeded++;
                        }
                        break;
                    default:
                        errors.Add(new BulkItemError(id, $"Unknown action: {request.Action}"));
                        break;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new BulkItemError(id, ex.Message));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new BulkActionResult(succeeded, errors.Count, errors);
    }
}
