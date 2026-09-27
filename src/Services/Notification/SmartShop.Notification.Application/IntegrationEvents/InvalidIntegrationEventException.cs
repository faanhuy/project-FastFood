namespace SmartShop.Notification.Application.IntegrationEvents;

public class InvalidIntegrationEventException : Exception
{
    public InvalidIntegrationEventException(string message) : base(message) { }
    public InvalidIntegrationEventException(string message, Exception inner) : base(message, inner) { }
}
