using MediatR;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Admin.Users.Queries.GetUsers;

public record GetUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? RoleFilter = null,
    bool? BannedFilter = null,
    string? SearchEmail = null
) : IRequest<PagedResult<UserDto>>;
