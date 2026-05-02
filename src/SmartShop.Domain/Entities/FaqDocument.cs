using SmartShop.Domain.Common;

namespace SmartShop.Domain.Entities;

public class FaqDocument : BaseAuditableEntity
{
    public string Category { get; private set; } = string.Empty;   // "shipping" | "returns" | "payment" | "general"
    public string Question { get; private set; } = string.Empty;
    public string Answer { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private FaqDocument() { }

    public static FaqDocument Create(string category, string question, string answer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        ArgumentException.ThrowIfNullOrWhiteSpace(answer);

        return new FaqDocument
        {
            Category = category.ToLowerInvariant(),
            Question = question,
            Answer = answer,
            IsActive = true
        };
    }

    public void Update(string category, string question, string answer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        ArgumentException.ThrowIfNullOrWhiteSpace(answer);

        Category = category.ToLowerInvariant();
        Question = question;
        Answer = answer;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
