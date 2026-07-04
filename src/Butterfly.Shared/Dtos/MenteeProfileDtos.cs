using System.ComponentModel.DataAnnotations;
using Butterfly.Shared.Enums;

namespace Butterfly.Shared.Dtos;

/// <summary>
/// A mentee profile as returned to clients. SAFEGUARDING: carries only a display name / pseudonym
/// and a region (village-level) — never a legal name, address, or GPS coordinate. Mentors only ever
/// receive profiles whose <see cref="ProfileStatus"/> is <see cref="ProfileStatus.Approved"/>.
/// </summary>
public sealed record MenteeProfileDto
{
    public required Guid Id { get; init; }

    /// <summary>First name or pseudonym only.</summary>
    public required string DisplayName { get; init; }

    public required int Age { get; init; }

    /// <summary>Region / village-level location only.</summary>
    public required string Region { get; init; }

    public required TalentCategory TalentCategory { get; init; }
    public required string Story { get; init; }
    public string? PhotoUrl { get; init; }
    public required IReadOnlyList<string> Tags { get; init; }
    public required SupportNeeded SupportNeeded { get; init; }

    /// <summary>Monthly need in BDT. Null when guidance-only (no financial need).</summary>
    public decimal? MonthlyNeedBDT { get; init; }

    public required ProfileStatus ProfileStatus { get; init; }
    public required Guid CreatedByCareManagerId { get; init; }
    public Guid? ApprovedByAdminId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Create a mentee profile (<c>POST /api/caremanagers/profiles</c>). Always saved <c>Pending</c>.
/// Validation enforces the safeguarding shape at the contract boundary.
/// </summary>
public sealed record CreateMenteeProfileRequestDto
{
    [Required, StringLength(60, MinimumLength = 1)]
    public required string DisplayName { get; init; }

    [Range(1, 17, ErrorMessage = "Mentees are minors; age must be between 1 and 17.")]
    public required int Age { get; init; }

    [Required, StringLength(120, MinimumLength = 1)]
    public required string Region { get; init; }

    [Required, EnumDataType(typeof(TalentCategory))]
    public required TalentCategory TalentCategory { get; init; }

    [Required, StringLength(2000, MinimumLength = 1)]
    public required string Story { get; init; }

    [Url, StringLength(500)]
    public string? PhotoUrl { get; init; }

    [Required, MinLength(1), MaxLength(20)]
    public required IReadOnlyList<string> Tags { get; init; }

    [Required, EnumDataType(typeof(SupportNeeded))]
    public required SupportNeeded SupportNeeded { get; init; }

    /// <summary>Required when <see cref="SupportNeeded"/> is Financial/Both; must be null for Mentorship-only.</summary>
    [Range(0.01, 1_000_000)]
    public decimal? MonthlyNeedBDT { get; init; }
}
