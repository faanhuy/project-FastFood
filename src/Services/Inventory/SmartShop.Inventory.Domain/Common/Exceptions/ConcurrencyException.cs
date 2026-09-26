namespace SmartShop.Inventory.Domain.Common.Exceptions;

public class ConcurrencyException(string message) : Exception(message);
