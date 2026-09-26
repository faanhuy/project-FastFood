namespace SmartShop.Inventory.Application.IntegrationEvents;

/// <summary>
/// Message không thể xử lý vì chính nội dung của nó (JSON hỏng, thiếu OrderId, topic lạ). Thử lại bao nhiêu lần
/// cũng vẫn hỏng nên consumer bỏ qua message thay vì retry mãi làm kẹt partition.
/// </summary>
public class InvalidIntegrationEventException(string message, Exception? innerException = null)
    : Exception(message, innerException);
