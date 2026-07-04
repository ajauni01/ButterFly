using Butterfly.Shared.Enums;

namespace Butterfly.Shared.Dtos;

/// <summary>
/// The current user's app-side profile, returned by <c>GET /api/me</c>.
/// No credentials are ever included — Entra owns authentication.
/// </summary>
public sealed record UserProfileDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required UserRole Role { get; init; }

    /// <summary>Set when <see cref="Role"/> is <see cref="UserRole.Mentor"/>; else null.</summary>
    public Guid? MentorId { get; init; }

    /// <summary>Set when <see cref="Role"/> is <see cref="UserRole.CareManager"/>; else null.</summary>
    public Guid? CareManagerId { get; init; }

    /// <summary>True once a mentor has completed the values/interests survey.</summary>
    public bool HasCompletedSurvey { get; init; }

    /// <summary>For care managers: whether an Admin has verified them.</summary>
    public bool? IsVerifiedCareManager { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
