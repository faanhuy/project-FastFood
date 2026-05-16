using MediatR;
using Microsoft.AspNetCore.Http;

namespace SmartShop.Application.Features.Combos.Commands.UploadComboImage;

public record UploadComboImageCommand(IFormFile File) : IRequest<UploadComboImageResult>;

public record UploadComboImageResult(string Url);
