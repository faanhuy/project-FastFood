namespace SmartShop.Domain.Common.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }

    public UnauthorizedException(string messageKey, Dictionary<string, string>? parameters)
        : base(messageKey)
    {
        MessageKey = messageKey;
        Params = parameters;
    }

    public string? MessageKey { get; }
    public Dictionary<string, string>? Params { get; }
}
