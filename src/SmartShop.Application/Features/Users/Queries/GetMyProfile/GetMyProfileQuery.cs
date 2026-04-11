using MediatR;

namespace SmartShop.Application.Features.Users.Queries.GetMyProfile;

public record GetMyProfileQuery(Guid UserId) : IRequest<UserProfileDto>;
