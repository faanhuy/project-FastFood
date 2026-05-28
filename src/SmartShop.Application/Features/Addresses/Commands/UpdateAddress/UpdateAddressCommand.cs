using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Addresses;

namespace SmartShop.Application.Features.Addresses.Commands.UpdateAddress;

public record UpdateAddressRequest(
    string Label,
    string RecipientName,
    string Phone,
    string Street,
    int? ProvinceId = null,
    int? WardId = null);

public record UpdateAddressCommand(
    Guid AddressId,
    Guid UserId,
    string Label,
    string RecipientName,
    string Phone,
    string Street,
    int? ProvinceId = null,
    int? WardId = null) : IRequest<ApiResponse<AddressDto>>;
