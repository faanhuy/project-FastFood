using SmartShop.Application.Common.Interfaces;
using SmartShop.Domain.Entities;

namespace SmartShop.Infrastructure.Services;

public class ChatbotService(ISemanticKernelService ai) : IChatbotService
{
    public async Task<string> GenerateReplyAsync(
        string userMessage,
        IReadOnlyList<FaqDocument> context,
        IReadOnlyList<(string Role, string Content)> history,
        CancellationToken ct = default)
    {
        // 1. Build context string từ FAQs
        var contextStr = context.Count > 0
            ? string.Join("\n\n", context.Select(f =>
                $"[{f.Category.ToUpper()}] Q: {f.Question}\nA: {f.Answer}"))
            : "(Không có tài liệu)";

        // 2. Build history string từ messages gần nhất
        var historyStr = history.Count > 0
            ? string.Join("\n", history.Select(h =>
                $"{(h.Role == "user" ? "Khách" : "Bot")}: {h.Content}"))
            : string.Empty;

        // 3. Xây dựng system prompt
        var systemPrompt = $"""
            Bạn là trợ lý ảo của FastFood, cửa hàng đồ ăn nhanh trực tuyến.
            Chỉ trả lời dựa trên thông tin trong phần TÀI LIỆU bên dưới.
            Nếu không có thông tin liên quan, nói: "Xin lỗi, tôi không có thông tin về vấn đề này. Vui lòng liên hệ hotline 1900-xxxx để được hỗ trợ."
            Trả lời ngắn gọn, thân thiện, bằng tiếng Việt.

            TÀI LIỆU:
            {contextStr}

            LỊCH SỬ HỘI THOẠI:
            {historyStr}
            """;

        // 4. Gọi LLM qua interface
        return await ai.ChatAsync(systemPrompt, userMessage, ct);
    }
}
