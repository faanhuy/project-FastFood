using MediatR;
using SmartShop.Application.Common.Models;
using SmartShop.Application.Features.Stores.Queries.GetStores;

namespace SmartShop.Application.Features.Stores.Queries.GetAllStoresAdmin;

public record GetAllStoresAdminQuery : IRequest<ApiResponse<List<AdminStoreDto>>>;

public record AdminStoreDto(
    Guid Id,
    string Name,
    string Address,
    string Phone,
    bool IsActive,
    string? Street = null,
    int? ProvinceId = null,
    int? WardId = null,
    string? ProvinceName = null,
    string? WardName = null);
