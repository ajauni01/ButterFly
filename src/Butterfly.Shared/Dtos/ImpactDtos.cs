using System.ComponentModel.DataAnnotations;
using Butterfly.Shared.Enums;

namespace Butterfly.Shared.Dtos;

/// <summary>
/// Log a weekly impact update against a mentorship (<c>POST /api/mentorships/{id}/impact</c>).
/// Authored by the care manager who manages the mentee; mentors read but never write.
/// Which optional fields are expected depends on <see cref="UpdateType"/>.
/// </summary>
public sealed record CreateImpactUpdateRequestDto
{
    [Required]
    public required DateOnly WeekOf { get; init; }

    [Required, EnumDataType(typeof(ImpactUpdateType))]
    public required ImpactUpdateType UpdateType { get; init; }

    /// <summary>For <see cref="ImpactUpdateType.Spend"/>: what the money was used for.</summary>
    [StringLength(1000)]
    public string? SpendDescription { get; init; }

    /// <summary>For <see cref="ImpactUpdateType.Spend"/>: amount spent in BDT.</summary>
    [Range(0.01, 1_000_000)]
    public decimal? AmountSpentBDT { get; init; }

    /// <summary>For <see cref="ImpactUpdateType.Session"/>: summary of an overseen mentor↔mentee meeting.</summary>
    [StringLength(2000)]
    public string? SessionSummary { get; init; }

    [Required, StringLength(2000, MinimumLength = 1)]
    public required string ImpactNote { get; init; }

    [Url, StringLength(500)]
    public string? PhotoUrl { get; init; }
}

/// <summary>A single impact update in a mentorship's feed.</summary>
public sealed record ImpactUpdateDto
{
    public required Guid Id { get; init; }
    public required Guid MentorshipId { get; init; }
    public required Guid CareManagerId { get; init; }
    public required DateOnly WeekOf { get; init; }
    public required ImpactUpdateType UpdateType { get; init; }
    public string? SpendDescription { get; init; }
    public decimal? AmountSpentBDT { get; init; }
    public string? SessionSummary { get; init; }
    public required string ImpactNote { get; init; }
    public string? PhotoUrl { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
