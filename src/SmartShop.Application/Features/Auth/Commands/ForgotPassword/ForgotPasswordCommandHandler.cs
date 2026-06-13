using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IEmailService emailService,
    IUnitOfWork unitOfWork
) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Silently succeed when the email is unknown or the account is banned,
        // so the endpoint never reveals whether an email is registered.
        if (user is null || user.IsBanned)
            return;

        var tempPassword = GenerateTempPassword();
        var hash = passwordHasher.Hash(tempPassword);
        user.UpdatePassword(hash);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await emailService.SendPasswordResetAsync(
            user.Email,
            $"{user.FirstName} {user.LastName}",
            tempPassword);
    }

    private static string GenerateTempPassword()
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        return new string(Enumerable.Range(0, 12).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }
}
