using MediatR;
using SmartShop.Application.Common.Exceptions;
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
            throw new UnauthorizedException("Refresh token không hợp lệ.");

        user.RevokeRefreshToken();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
