using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Stores.Queries.GetAllStoresAdmin;

public class GetAllStoresAdminQueryHandler(IStoreRepository storeRepository)
    : IRequestHandler<GetAllStoresAdminQuery, ApiResponse<List<AdminStoreDto>>>
{
    public async Task<ApiResponse<List<AdminStoreDto>>> Handle(GetAllStoresAdminQuery request, CancellationToken cancellationToken)
    {
        var stores = await storeRepository.GetAllAsync(cancellationToken);

        var dtos = stores.Select(s => new AdminStoreDto(
            s.Id, s.Name, s.Address, s.Phone, s.IsActive,
            s.Street, s.ProvinceId, s.WardId,
            s.Province?.Name, s.Ward?.Name)).ToList();

        return ApiResponse<List<AdminStoreDto>>.Ok(dtos);
    }
}
