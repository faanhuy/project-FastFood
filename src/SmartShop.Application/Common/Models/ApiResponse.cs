namespace SmartShop.Application.Common.Models;

public sealed class ApiResponse<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public string? Message { get; }
    public IReadOnlyList<string> Errors { get; }

    private ApiResponse(bool success, T? data, string? message, IReadOnlyList<string> errors)
    {
        Success = success;
        Data = data;
        Message = message;
        Errors = errors;
    }

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new(true, data, message, []);

    public static ApiResponse<T> Fail(IEnumerable<string> errors)
        => new(false, default, null, errors.ToList());

    public static ApiResponse<T> Fail(string error)
        => new(false, default, null, [error]);
}

public static class ApiResponse
{
    public static ApiResponse<object?> Ok(string? message = null)
        => ApiResponse<object?>.Ok(null, message);

    public static ApiResponse<object?> Fail(IEnumerable<string> errors)
        => ApiResponse<object?>.Fail(errors);

    public static ApiResponse<object?> Fail(string error)
        => ApiResponse<object?>.Fail(error);
}
