namespace Butterfly.Api.Infrastructure;

/// <summary>
/// Thrown by controllers/services to signal a specific HTTP failure with a stable error code.
/// Translated to an <c>ErrorDto</c> response by <see cref="ExceptionHandlingMiddleware"/>.
/// </summary>
public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }

    public ApiException(int statusCode, string code, string message) : base(message)
    {
        StatusCode = statusCode;
        Code = code;
    }

    public static ApiException NotFound(string message) =>
        new(StatusCodes.Status404NotFound, "not_found", message);

    public static ApiException Forbidden(string message) =>
        new(StatusCodes.Status403Forbidden, "forbidden", message);

    public static ApiException BadRequest(string message) =>
        new(StatusCodes.Status400BadRequest, "bad_request", message);

    public static ApiException Conflict(string message) =>
        new(StatusCodes.Status409Conflict, "conflict", message);
}
