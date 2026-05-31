using MediatR;

namespace SmartShop.Application.Features.Products.Commands.SetPrimaryImage;

public record SetPrimaryImageCommand(Guid ProductId, Guid ImageId) : IRequest;
