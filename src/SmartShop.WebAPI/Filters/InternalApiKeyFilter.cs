namespace SmartShop.WebAPI.Filters;

// Bảo vệ endpoint nội bộ (service-to-service, không qua JWT) bằng header shared-secret.
// Cổng 8080 của Core đã public ra host cho REST thông thường nên network isolation không đủ —
// xem docs/architecture/microservices-boundaries.md.
public class InternalApiKeyFilter(IConfiguration configuration) : IEndpointFilter
{
    private const string HeaderName = "X-Internal-Api-Key";

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var expectedKey = configuration["Internal:NotifyApiKey"];
        if (string.IsNullOrEmpty(expectedKey))
            return Results.Problem("Internal API key is not configured.", statusCode: StatusCodes.Status503ServiceUnavailable);

        var providedKey = context.HttpContext.Request.Headers[HeaderName].ToString();
        if (!string.Equals(providedKey, expectedKey, StringComparison.Ordinal))
            return Results.Unauthorized();

        return await next(context);
    }
}
