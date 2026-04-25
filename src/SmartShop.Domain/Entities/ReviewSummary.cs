namespace SmartShop.Domain.Entities;

public record ReviewSummary(
    Guid Id,
    Guid UserId,
    string UserFullName,
    Guid ProductId,
    int Rating,
    string Comment,
    DateTime CreatedAt
);
