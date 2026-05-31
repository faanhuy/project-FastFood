namespace SmartShop.Domain.Common.Exceptions;

public class NotFoundException(string entityName, object key)
    : Exception($"{entityName} với Id '{key}' không tìm thấy.")
{
    public string EntityName { get; } = entityName;
    public object Key { get; } = key;
}
