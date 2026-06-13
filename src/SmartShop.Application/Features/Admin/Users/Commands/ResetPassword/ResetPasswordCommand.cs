using MediatR;

namespace SmartShop.Application.Features.Admin.Users.Commands.ResetPassword;

public record ResetPasswordCommand(Guid TargetUserId) : IRequest<ResetPasswordResult>;

public record ResetPasswordResult(string Message);
