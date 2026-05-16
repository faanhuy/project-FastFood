using MediatR;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Common.Utilities;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Users.Commands.UploadAvatar;

public class UploadUserAvatarCommandHandler(
    IUserRepository userRepository,
    IFileStorageService fileStorage,
    IUnitOfWork unitOfWork
) : IRequestHandler<UploadUserAvatarCommand, UploadUserAvatarResult>
{
    public async Task<UploadUserAvatarResult> Handle(
        UploadUserAvatarCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var safeFileName = FileSecurityHelper.BuildStorageFileName(
            request.File.FileName,
            request.UserId.ToString()
        );

        // Upload file mới
        var newUrl = await fileStorage.UploadAsync(
            request.File.OpenReadStream(),
            safeFileName,
            UploadCategory.UserAvatar,
            cancellationToken
        );

        // Xóa avatar cũ nếu có (silent fail)
        if (!string.IsNullOrEmpty(user.AvatarUrl))
            await fileStorage.DeleteAsync(user.AvatarUrl, UploadCategory.UserAvatar, cancellationToken);

        // Cập nhật entity
        user.SetAvatarUrl(newUrl);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadUserAvatarResult(newUrl);
    }
}
