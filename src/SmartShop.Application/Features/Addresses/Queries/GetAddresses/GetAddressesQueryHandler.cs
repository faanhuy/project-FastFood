using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Addresses.Queries.GetAddresses;

public class GetAddressesQueryHandler(
    IUserAddressRepository addressRepository) : IRequestHandler<GetAddressesQuery, ApiResponse<List<AddressDto>>>
{
    public async Task<ApiResponse<List<AddressDto>>> Handle(GetAddressesQuery query, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(query.UserId);
        var addresses = await addressRepository.GetByUserIdAsync(userId, cancellationToken);

        var dtos = addresses
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAt)
            .Select(AddressDto.From)
            .ToList();

        return ApiResponse<List<AddressDto>>.Ok(dtos);
    }
}
