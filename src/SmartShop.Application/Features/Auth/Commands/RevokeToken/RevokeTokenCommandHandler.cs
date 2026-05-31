using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RevokeTokenCommand>
{
    public async Task Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user is null)
            throw new UnauthorizedException("error.auth_refresh_token_revoked", null);

        user.RevokeRefreshToken();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
