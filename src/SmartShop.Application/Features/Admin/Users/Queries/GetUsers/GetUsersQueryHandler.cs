using MediatR;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Admin.Users.Queries.GetUsers;

public class GetUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var paged = await userRepository.GetPagedAsync(
            request.Page, request.PageSize,
            request.RoleFilter, request.BannedFilter, request.SearchEmail,
            request.SortBy, request.SortDirection, cancellationToken);

        var dtos = paged.Items.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role,
            IsBanned = u.IsBanned,
            BannedAt = u.BannedAt,
            CreatedAt = u.CreatedAt,
            OrderCount = 0
        }).ToList();

        return new PagedResult<UserDto>(dtos, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
