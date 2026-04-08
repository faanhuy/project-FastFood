namespace SmartShop.Application.Common.Exceptions;

public class NotFoundException(string entityName, object key)
    : Exception($"{entityName} với Id '{key}' không tìm thấy.");
