using System.Net;
using System.Text.Json;
using FluentValidation;
using SmartShop.Application.Common.Exceptions;
using SmartShop.Application.Common.Models;

namespace SmartShop.WebAPI.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                ve.Errors.Select(e => e.ErrorMessage).ToList()
            ),
            ConflictException ce => (HttpStatusCode.Conflict, new List<string> { ce.Message }),
            UnauthorizedException ue => (HttpStatusCode.Unauthorized, new List<string> { ue.Message }),
            _ => (HttpStatusCode.InternalServerError, new List<string> { "Đã xảy ra lỗi hệ thống." })
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.Fail(errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
