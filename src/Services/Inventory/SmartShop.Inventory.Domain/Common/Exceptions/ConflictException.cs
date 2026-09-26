namespace SmartShop.Inventory.Domain.Common.Exceptions;

// Luôn dùng constructor 2-param với message key (error.inventory_*) — không có overload 1-param.
public class ConflictException : Exception
{
    public ConflictException(string messageKey, Dictionary<string, string>? parameters)
        : base(messageKey)
    {
        MessageKey = messageKey;
        Params = parameters;
    }

    public string? MessageKey { get; }
    public Dictionary<string, string>? Params { get; }
}
