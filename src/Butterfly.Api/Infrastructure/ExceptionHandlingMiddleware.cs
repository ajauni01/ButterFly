namespace Butterfly.Api.Infrastructure;

/// <summary>
/// Converts unhandled exceptions into the consistent ErrorDto envelope.
/// <see cref="ApiException"/> maps to its declared status/code; anything else becomes a 500 whose
/// details are logged but never leaked to the client.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogInformation(ex, "Handled API error {Code} ({Status})", ex.Code, ex.StatusCode);
            await ErrorResponse.WriteAsync(context.Response, ex.StatusCode, ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await ErrorResponse.WriteAsync(context.Response, StatusCodes.Status500InternalServerError,
                "server_error", "An unexpected error occurred.");
        }
    }
}
