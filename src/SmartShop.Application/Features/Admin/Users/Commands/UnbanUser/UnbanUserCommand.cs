using MediatR;

namespace SmartShop.Application.Features.Admin.Users.Commands.UnbanUser;

public record UnbanUserCommand(Guid TargetUserId) : IRequest<UserDto>;
