using MediatR;

namespace SmartShop.Application.Features.Admin.Users.Queries.GetUserDetail;

public record GetUserDetailQuery(Guid UserId) : IRequest<UserDto>;
