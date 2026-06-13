using MediatR;

namespace SmartShop.Application.Features.Admin.Users.Commands.ForceLogout;

public record ForceLogoutCommand(Guid TargetUserId) : IRequest<ForceLogoutResult>;

public record ForceLogoutResult(string Message, int RevokedTokenCount);
