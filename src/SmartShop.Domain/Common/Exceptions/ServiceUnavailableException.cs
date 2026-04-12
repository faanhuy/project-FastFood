namespace SmartShop.Domain.Common.Exceptions;

public class ServiceUnavailableException(string message) : Exception(message);
