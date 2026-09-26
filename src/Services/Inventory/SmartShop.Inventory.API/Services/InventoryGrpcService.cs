using Grpc.Core;
using MediatR;
using SmartShop.Contracts.Grpc.Inventory;
using SmartShop.Inventory.Application.Features.Stock.Commands.ConfirmStock;
using SmartShop.Inventory.Application.Features.Stock.Commands.ReleaseStock;
using SmartShop.Inventory.Application.Features.Stock.Commands.ReserveStock;
using SmartShop.Inventory.Application.Features.Stock.Queries.GetStockLevel;

namespace SmartShop.Inventory.API.Services;

/// <summary>
/// Lớp mỏng: chỉ dịch proto ↔ Command/Query (string ↔ Guid) rồi gọi mediator.
/// Không chứa nghiệp vụ; lỗi domain được ExceptionInterceptor đổi thành gRPC status.
/// </summary>
public class InventoryGrpcService(IMediator mediator) : InventoryGrpc.InventoryGrpcBase
{
    public override async Task<ReserveStockResponse> CheckAndReserveStock(
        ReserveStockRequest request, ServerCallContext context)
    {
        var command = new ReserveStockCommand(
            ParseGuid(request.OrderId, "order_id"),
            request.Items.Select(i => new ReserveStockLine(
                ParseGuid(i.ProductId, "product_id"),
                ParseGuid(i.StoreId, "store_id"),
                i.HasSizeId ? ParseGuid(i.SizeId, "size_id") : null,
                i.Quantity)).ToList());

        var result = await mediator.Send(command, context.CancellationToken);

        var response = new ReserveStockResponse { Success = result.Success };
        response.Results.AddRange(result.Results.Select(r =>
        {
            var item = new StockItemResult
            {
                ProductId = r.ProductId.ToString(),
                Reserved = r.Reserved,
                AvailableQuantity = r.AvailableQuantity
            };

            // optional field: chỉ gán khi có giá trị để HasSizeId phản ánh đúng "có size / không size"
            if (r.SizeId.HasValue)
                item.SizeId = r.SizeId.Value.ToString();

            return item;
        }));

        return response;
    }

    public override async Task<ReleaseStockResponse> ReleaseStock(
        ReleaseStockRequest request, ServerCallContext context)
    {
        await mediator.Send(
            new ReleaseStockCommand(ParseGuid(request.OrderId, "order_id")),
            context.CancellationToken);

        return new ReleaseStockResponse { Success = true };
    }

    public override async Task<ConfirmStockResponse> ConfirmStock(
        ConfirmStockRequest request, ServerCallContext context)
    {
        await mediator.Send(
            new ConfirmStockCommand(ParseGuid(request.OrderId, "order_id")),
            context.CancellationToken);

        return new ConfirmStockResponse { Success = true };
    }

    public override async Task<GetStockLevelResponse> GetStockLevel(
        GetStockLevelRequest request, ServerCallContext context)
    {
        var level = await mediator.Send(
            new GetStockLevelQuery(
                ParseGuid(request.ProductId, "product_id"),
                ParseGuid(request.StoreId, "store_id"),
                request.HasSizeId ? ParseGuid(request.SizeId, "size_id") : null),
            context.CancellationToken);

        return new GetStockLevelResponse { AvailableQuantity = level };
    }

    private static Guid ParseGuid(string value, string field)
        => Guid.TryParse(value, out var id)
            ? id
            : throw new RpcException(new Status(StatusCode.InvalidArgument, $"'{field}' is not a valid GUID."));
}
