using System.Net;
using System.Text.Json;
using FluentValidation;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Application.Common.Models;
using SmartShop.Domain.Interfaces;

namespace SmartShop.WebAPI.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment env,
    IServiceScopeFactory scopeFactory)
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
            await HandleExceptionAsync(context, ex, env.IsDevelopment());
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
    {
        context.Response.ContentType = "application/json";

        // Detect language from Accept-Language header
        var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();
        string lang;
        List<string> errors;

        using (var scope = scopeFactory.CreateScope())
        {
            var localization = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
            lang = localization.DetectLanguage(acceptLanguage);

            List<string> internalErrors = isDevelopment
                ? new[] { $"{exception.GetType().Name}: {exception.Message}", exception.InnerException?.Message ?? "" }
                    .Where(s => !string.IsNullOrEmpty(s)).ToList()
                : new List<string> { localization.GetMessage("error.internal_server_error", lang) };

            var (statusCode, errorList) = exception switch
            {
                ValidationException ve => (
                    HttpStatusCode.BadRequest,
                    ve.Errors.Select(e =>
                        e.ErrorMessage.StartsWith("validation.")
                            ? localization.GetMessage(e.ErrorMessage, lang)
                            : e.ErrorMessage
                    ).ToList()
                ),
                NotFoundException nfe => (
                    HttpStatusCode.NotFound,
                    new List<string>
                    {
                        localization.GetMessage("error.not_found", lang,
                            new Dictionary<string, string>
                            {
                                ["entity"] = nfe.EntityName,
                                ["id"] = nfe.Key.ToString() ?? ""
                            })
                    }
                ),
                ConflictException ce => (
                    HttpStatusCode.Conflict,
                    new List<string>
                    {
                        ce.MessageKey != null
                            ? localization.GetMessage(ce.MessageKey, lang, ce.Params)
                            : ce.Message
                    }
                ),
                ConcurrencyException cce => (
                    HttpStatusCode.Conflict,
                    new List<string> { localization.GetMessage("error.concurrency", lang) }
                ),
                UnauthorizedException ue => (
                    HttpStatusCode.Unauthorized,
                    new List<string>
                    {
                        ue.MessageKey != null
                            ? localization.GetMessage(ue.MessageKey, lang, ue.Params)
                            : localization.GetMessage("error.unauthorized", lang)
                    }
                ),
                ServiceUnavailableException => (
                    HttpStatusCode.ServiceUnavailable,
                    new List<string> { localization.GetMessage("error.service_unavailable", lang) }
                ),
                _ => (HttpStatusCode.InternalServerError, internalErrors)
            };

            context.Response.StatusCode = (int)statusCode;
            errors = errorList;
        }

        var response = ApiResponse.Fail(errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
