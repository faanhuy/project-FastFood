using MediatR;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Admin.Users.Commands.ForceLogout;

public class ForceLogoutCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<ForceLogoutCommand, ForceLogoutResult>
{
    public async Task<ForceLogoutResult> Handle(ForceLogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.TargetUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.TargetUserId);

        var hadToken = user.RefreshTokenHash != null || user.RefreshToken != null;
        user.RevokeRefreshToken();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ForceLogoutResult("Tokens revoked successfully", hadToken ? 1 : 0);
    }
}
