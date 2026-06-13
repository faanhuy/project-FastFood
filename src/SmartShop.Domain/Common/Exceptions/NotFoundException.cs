namespace SmartShop.Domain.Common.Exceptions;

public class NotFoundException(string entityName, object key)
    : Exception($"{entityName} with Id '{key}' was not found.")
{
    public string EntityName { get; } = entityName;
    public object Key { get; } = key;
}
