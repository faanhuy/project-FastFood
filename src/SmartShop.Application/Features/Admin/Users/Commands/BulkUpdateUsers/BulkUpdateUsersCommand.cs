using MediatR;
using SmartShop.Application.Features.Common;

namespace SmartShop.Application.Features.Admin.Users.Commands.BulkUpdateUsers;

public record BulkUpdateUsersCommand(
    List<Guid> UserIds,
    string Action,
    Guid AdminUserId,
    string? RoleValue = null
) : IRequest<BulkActionResult>;
