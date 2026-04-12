using MediatR;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Interfaces;
using SmartShop.Domain.Entities;
using SmartShop.Domain.Interfaces;

namespace SmartShop.Application.Features.Users.Commands.UpdateMyProfile;

public class UpdateMyProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateMyProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        user.UpdateProfile(request.FirstName, request.LastName);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserProfileDto
        {
            Id        = user.Id,
            Email     = user.Email,
            FirstName = user.FirstName,
            LastName  = user.LastName,
            Role      = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
}
