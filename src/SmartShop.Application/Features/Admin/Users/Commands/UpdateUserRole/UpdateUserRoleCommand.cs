using MediatR;

namespace SmartShop.Application.Features.Admin.Users.Commands.UpdateUserRole;

public record UpdateUserRoleCommand(Guid TargetUserId, Guid RequestingUserId, string NewRole) : IRequest<UserDto>;
