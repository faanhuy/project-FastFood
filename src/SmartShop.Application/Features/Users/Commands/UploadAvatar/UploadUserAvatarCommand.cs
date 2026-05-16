using MediatR;
using Microsoft.AspNetCore.Http;

namespace SmartShop.Application.Features.Users.Commands.UploadAvatar;

public record UploadUserAvatarCommand(Guid UserId, IFormFile File) : IRequest<UploadUserAvatarResult>;

public record UploadUserAvatarResult(string Url);
