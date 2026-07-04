using Butterfly.Shared.Enums;

namespace Butterfly.Shared.Dtos;

/// <summary>A care manager's app-side profile.</summary>
public sealed record CareManagerDto
{
    public required Guid Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Region { get; init; }

    /// <summary>Admin-set: only verified care managers should be trusted middlemen.</summary>
    public required bool IsVerified { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
