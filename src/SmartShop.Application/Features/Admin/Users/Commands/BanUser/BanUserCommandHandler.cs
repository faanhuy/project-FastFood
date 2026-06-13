using MediatR;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Admin.Users.Commands.BanUser;

public class BanUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<BanUserCommand, UserDto>
{
    public async Task<UserDto> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.TargetUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.TargetUserId);

        if (request.TargetUserId == request.RequestingUserId)
            throw new ConflictException("error.user_cannot_self_ban", null);

        if (user.Role == "Admin")
            throw new ConflictException("error.user_cannot_ban_admin", null);

        if (user.IsBanned)
            throw new ConflictException("error.user_already_banned", null);

        user.Ban();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role,
        IsBanned = user.IsBanned,
        BannedAt = user.BannedAt,
        CreatedAt = user.CreatedAt
    };
}
