using MediatR;

namespace SmartShop.Application.Features.Products.Commands.DeleteProductImage;

public record DeleteProductImageCommand(Guid ProductId, Guid ImageId) : IRequest;
