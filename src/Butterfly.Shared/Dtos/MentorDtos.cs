using System.ComponentModel.DataAnnotations;
using Butterfly.Shared.Enums;

namespace Butterfly.Shared.Dtos;

/// <summary>
/// Submit or replace a mentor's values/interests survey (<c>POST /api/mentors/survey</c>).
/// Tags drive the tag-overlap matching against approved mentee profiles.
/// </summary>
public sealed record SurveyRequestDto
{
    [Required, MinLength(1, ErrorMessage = "Pick at least one value.")]
    [MaxLength(20)]
    public required IReadOnlyList<string> Values { get; init; }

    [Required, MinLength(1, ErrorMessage = "Pick at least one interest.")]
    [MaxLength(20)]
    public required IReadOnlyList<string> Interests { get; init; }

    /// <summary>Optional preferred talent category to bias matching; null means no preference.</summary>
    public TalentCategory? PreferredTalentCategory { get; init; }
}

/// <summary>A mentor's stored survey response.</summary>
public sealed record SurveyResponseDto
{
    public required Guid Id { get; init; }
    public required IReadOnlyList<string> Values { get; init; }
    public required IReadOnlyList<string> Interests { get; init; }
    public TalentCategory? PreferredTalentCategory { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// One matched mentee for a mentor (<c>GET /api/mentors/matches</c>): an approved profile plus
/// the computed tag-overlap score. Only ever wraps <see cref="ProfileStatus.Approved"/> profiles.
/// </summary>
public sealed record MenteeMatchDto
{
    public required MenteeProfileDto Profile { get; init; }

    /// <summary>Number of overlapping tags between the mentor's survey and the profile.</summary>
    public required int MatchScore { get; init; }

    /// <summary>The specific tags that matched — lets the UI explain "why" this match surfaced.</summary>
    public required IReadOnlyList<string> MatchedTags { get; init; }
}
