namespace SmartShop.Domain.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }

    public ConflictException(string messageKey, Dictionary<string, string>? parameters)
        : base(messageKey)
    {
        MessageKey = messageKey;
        Params = parameters;
    }

    public string? MessageKey { get; }
    public Dictionary<string, string>? Params { get; }
}
