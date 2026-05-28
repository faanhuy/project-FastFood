using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Addresses;

namespace SmartShop.Application.Features.Addresses.Commands.AddAddress;

public record AddAddressRequest(
    string Label,
    string RecipientName,
    string Phone,
    string Street,
    int? ProvinceId = null,
    int? WardId = null);

public record AddAddressCommand(
    Guid UserId,
    string Label,
    string RecipientName,
    string Phone,
    string Street,
    int? ProvinceId = null,
    int? WardId = null) : IRequest<ApiResponse<AddressDto>>;
