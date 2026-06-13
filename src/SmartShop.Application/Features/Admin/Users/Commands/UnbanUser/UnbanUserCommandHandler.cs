using MediatR;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Admin.Users.Commands.UnbanUser;

public class UnbanUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UnbanUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UnbanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.TargetUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.TargetUserId);

        if (!user.IsBanned)
            throw new ConflictException("error.user_not_banned", null);

        user.Unban();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDto
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
}
