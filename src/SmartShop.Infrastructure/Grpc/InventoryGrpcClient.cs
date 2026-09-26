using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartShop.Application.Common.Interfaces;
using SmartShop.Contracts.Grpc.Inventory;
using SmartShop.Domain.Common.Exceptions;

namespace SmartShop.Infrastructure.Grpc;

/// <summary>
/// Bọc generated <see cref="InventoryGrpc.InventoryGrpcClient"/>: dịch record ↔ proto (Guid ↔ string),
/// áp deadline cứng cho mọi call và đổi <see cref="RpcException"/> thành exception domain của Core
/// để <c>ExceptionHandlingMiddleware</c> xử lý như mọi lỗi khác.
/// </summary>
public class InventoryGrpcClient(
    InventoryGrpc.InventoryGrpcClient client,
    IConfiguration configuration,
    ILogger<InventoryGrpcClient> logger) : IInventoryClient
{
    // Deadline cứng: Inventory chậm/chết thì fail rõ ràng, không để request của FE treo. Mặc định 3 giây.
    // Grpc:InventoryTimeoutSeconds cho phép nới ra khi debug Inventory (đứng ở breakpoint quá deadline là cuộc gọi bị hủy).
    private readonly TimeSpan _callTimeout =
        TimeSpan.FromSeconds(Math.Max(1, configuration.GetValue("Grpc:InventoryTimeoutSeconds", 3)));

    public async Task<ReserveStockOutcome> CheckAndReserveStockAsync(
        Guid orderId, IReadOnlyList<InventoryStockLine> items, CancellationToken ct = default)
    {
        var request = new ReserveStockRequest { OrderId = orderId.ToString() };
        request.Items.AddRange(items.Select(i =>
        {
            var item = new StockItem
            {
                ProductId = i.ProductId.ToString(),
                StoreId = i.StoreId.ToString(),
                Quantity = i.Quantity
            };

            // optional field: chỉ gán khi có size để phía server phân biệt "không size" với "có size"
            if (i.SizeId.HasValue)
                item.SizeId = i.SizeId.Value.ToString();

            return item;
        }));

        var response = await InvokeAsync(
            nameof(CheckAndReserveStockAsync), "StockReservation", orderId,
            deadline => client.CheckAndReserveStockAsync(request, deadline: deadline, cancellationToken: ct).ResponseAsync,
            ct);

        var results = response.Results.Select(r => new InventoryStockLineResult(
            Guid.Parse(r.ProductId),
            r.HasSizeId ? Guid.Parse(r.SizeId) : null,
            r.Reserved,
            r.AvailableQuantity)).ToList();

        return new ReserveStockOutcome(response.Success, results);
    }

    public async Task ReleaseStockAsync(Guid orderId, CancellationToken ct = default)
    {
        var request = new ReleaseStockRequest { OrderId = orderId.ToString() };

        await InvokeAsync(
            nameof(ReleaseStockAsync), "StockReservation", orderId,
            deadline => client.ReleaseStockAsync(request, deadline: deadline, cancellationToken: ct).ResponseAsync,
            ct);
    }

    public async Task ConfirmStockAsync(Guid orderId, CancellationToken ct = default)
    {
        var request = new ConfirmStockRequest { OrderId = orderId.ToString() };

        await InvokeAsync(
            nameof(ConfirmStockAsync), "StockReservation", orderId,
            deadline => client.ConfirmStockAsync(request, deadline: deadline, cancellationToken: ct).ResponseAsync,
            ct);
    }

    public async Task<int> GetStockLevelAsync(
        Guid productId, Guid storeId, Guid? sizeId, CancellationToken ct = default)
    {
        var request = new GetStockLevelRequest
        {
            ProductId = productId.ToString(),
            StoreId = storeId.ToString()
        };
        if (sizeId.HasValue)
            request.SizeId = sizeId.Value.ToString();

        var response = await InvokeAsync(
            nameof(GetStockLevelAsync), "InventoryItem",
            sizeId.HasValue ? $"{storeId}/{productId}/{sizeId}" : $"{storeId}/{productId}",
            deadline => client.GetStockLevelAsync(request, deadline: deadline, cancellationToken: ct).ResponseAsync,
            ct);

        return response.AvailableQuantity;
    }

    /// <summary>
    /// Chạy 1 call với deadline và dịch lỗi gRPC → exception domain:
    /// Unavailable/DeadlineExceeded → ServiceUnavailable (503) — fail-safe, không bao giờ coi như "còn hàng";
    /// NotFound → NotFound (404); FailedPrecondition → Conflict (409, Detail chính là message key của Inventory);
    /// các status khác (InvalidArgument, Internal…) để nguyên RpcException → middleware trả 500 và log.
    /// </summary>
    private async Task<TResponse> InvokeAsync<TResponse>(
        string operation,
        string notFoundEntity,
        object notFoundKey,
        Func<DateTime, Task<TResponse>> call,
        CancellationToken ct)
    {
        try
        {
            return await call(DateTime.UtcNow.Add(_callTimeout));
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled && ct.IsCancellationRequested)
        {
            // Caller huỷ (client ngắt kết nối) — không phải lỗi của Inventory
            throw new OperationCanceledException(ct);
        }
        catch (RpcException ex) when (ex.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded)
        {
            logger.LogError(ex, "Inventory gRPC {Operation} failed: {Status}", operation, ex.StatusCode);
            throw new ServiceUnavailableException($"Inventory service unavailable ({ex.StatusCode}).");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new NotFoundException(notFoundEntity, notFoundKey);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.FailedPrecondition)
        {
            throw new ConflictException(ex.Status.Detail, null);
        }
    }
}
