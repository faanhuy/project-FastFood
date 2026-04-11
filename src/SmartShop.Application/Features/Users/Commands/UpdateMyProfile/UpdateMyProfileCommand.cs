using MediatR;

namespace SmartShop.Application.Features.Users.Commands.UpdateMyProfile;

public record UpdateMyProfileCommand(Guid UserId, string FirstName, string LastName)
    : IRequest<UserProfileDto>;
