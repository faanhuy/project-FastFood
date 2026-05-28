using MediatR;
using SmartShop.Application.Common.Models;

namespace SmartShop.Application.Features.Addresses.Commands.DeleteAddress;

public record DeleteAddressCommand(Guid AddressId, Guid UserId) : IRequest<ApiResponse<bool>>;
