using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Admin.Users.Commands.ResetPassword;

public class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IEmailService emailService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    public async Task<ResetPasswordResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.TargetUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.TargetUserId);

        var tempPassword = GenerateTempPassword();
        var hash = passwordHasher.Hash(tempPassword);
        user.UpdatePassword(hash);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await emailService.SendPasswordResetAsync(user.Email, $"{user.FirstName} {user.LastName}", tempPassword);

        return new ResetPasswordResult("Password reset email sent");
    }

    private static string GenerateTempPassword()
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        return new string(Enumerable.Range(0, 12).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }
}
