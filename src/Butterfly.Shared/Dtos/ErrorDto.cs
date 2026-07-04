namespace Butterfly.Shared.Dtos;

/// <summary>
/// Consistent error envelope returned by every API failure path (400/401/403/404/…).
/// Keeps clients off ASP.NET's default problem-details shape so the MAUI app can bind one type.
/// </summary>
public sealed record ErrorDto
{
    /// <summary>Stable, machine-readable code (e.g. "not_found", "forbidden", "validation_failed").</summary>
    public required string Code { get; init; }

    /// <summary>Human-readable summary safe to show in the client.</summary>
    public required string Message { get; init; }

    /// <summary>Optional per-field validation messages, keyed by field name.</summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
}
