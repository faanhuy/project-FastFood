using MediatR;
using Microsoft.AspNetCore.Http;

namespace SmartShop.Application.Features.Reviews.Commands.AddReviewImages;

public record AddReviewImagesCommand(
    Guid ReviewId,
    Guid UserId,
    IList<IFormFile> Files
) : IRequest<AddReviewImagesResult>;

public record AddReviewImagesResult(
    List<string> Urls
);
