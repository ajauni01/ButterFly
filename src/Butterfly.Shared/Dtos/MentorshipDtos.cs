using System.ComponentModel.DataAnnotations;
using Butterfly.Shared.Enums;

namespace Butterfly.Shared.Dtos;

/// <summary>
/// Create a mentorship between the current mentor and an approved mentee
/// (<c>POST /api/mentorships</c>). A monthly USD commitment applies only to
/// Financial/Both relationships; it must be null for Guidance-only.
/// </summary>
public sealed record CreateMentorshipRequestDto
{
    [Required]
    public required Guid MenteeProfileId { get; init; }

    [Required, EnumDataType(typeof(RelationshipType))]
    public required RelationshipType RelationshipType { get; init; }

    /// <summary>Monthly commitment in USD. Required for Financial/Both, must be null for Guidance.</summary>
    [Range(1, 100_000)]
    public decimal? MonthlyAmountUSD { get; init; }

    [Required, EnumDataType(typeof(MeetingCadence))]
    public required MeetingCadence MeetingCadence { get; init; }
}

/// <summary>A mentorship as returned to the owning mentor.</summary>
public sealed record MentorshipDto
{
    public required Guid Id { get; init; }
    public required Guid MentorId { get; init; }
    public required Guid MenteeProfileId { get; init; }

    /// <summary>Denormalized mentee display name/pseudonym for convenient list rendering.</summary>
    public required string MenteeDisplayName { get; init; }

    public required RelationshipType RelationshipType { get; init; }
    public decimal? MonthlyAmountUSD { get; init; }
    public required MeetingCadence MeetingCadence { get; init; }
    public required DateOnly StartDate { get; init; }
    public required MentorshipStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
