using System.Text.Json;
using Butterfly.Shared.Dtos;

namespace Butterfly.Api.Infrastructure;

/// <summary>Helper to write the shared <see cref="ErrorDto"/> envelope directly to a response.</summary>
public static class ErrorResponse
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task WriteAsync(HttpResponse response, int status, string code, string message)
    {
        if (response.HasStarted)
            return;

        response.StatusCode = status;
        response.ContentType = "application/json";
        var dto = new ErrorDto { Code = code, Message = message };
        await response.WriteAsync(JsonSerializer.Serialize(dto, JsonOptions));
    }
}
