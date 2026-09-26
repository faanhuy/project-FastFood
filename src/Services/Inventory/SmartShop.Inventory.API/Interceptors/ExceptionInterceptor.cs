using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using SmartShop.Inventory.Domain.Common.Exceptions;

namespace SmartShop.Inventory.API.Interceptors;

/// <summary>
/// Đổi exception domain/application thành gRPC status (tương đương ExceptionHandlingMiddleware của Core).
/// Ánh xạ theo docs/architecture/microservices-boundaries.md.
/// </summary>
public class ExceptionInterceptor(ILogger<ExceptionInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;   // đã là gRPC status (vd InvalidArgument từ ParseGuid) — không bọc lại
        }
        catch (NotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (ConflictException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.MessageKey ?? ex.Message));
        }
        catch (ConcurrencyException ex)
        {
            throw new RpcException(new Status(StatusCode.Aborted, ex.Message));
        }
        catch (ValidationException ex)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))));
        }
        catch (Exception ex)
        {
            // Không lộ chi tiết nội bộ cho client
            logger.LogError(ex, "Unhandled error in gRPC method {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error."));
        }
    }
}
