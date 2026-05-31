using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Admin.Users.Queries.GetUserDetail;

public class GetUserDetailQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserDetailQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserDetailQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsBanned = user.IsBanned,
            BannedAt = user.BannedAt,
            CreatedAt = user.CreatedAt,
            OrderCount = 0
        };
    }
}
