using MediatR;

namespace SmartShop.Application.Features.Admin.Users.Commands.BanUser;

public record BanUserCommand(Guid TargetUserId, Guid RequestingUserId) : IRequest<UserDto>;
