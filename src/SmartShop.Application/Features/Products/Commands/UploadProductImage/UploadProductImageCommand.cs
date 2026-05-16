using MediatR;
using Microsoft.AspNetCore.Http;

namespace SmartShop.Application.Features.Products.Commands.UploadProductImage;

public record UploadProductImageCommand(IFormFile File) : IRequest<UploadProductImageResult>;

public record UploadProductImageResult(string Url);
