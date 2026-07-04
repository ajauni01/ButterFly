using Butterfly.Data.Entities;
using Butterfly.Shared.Enums;

namespace Butterfly.Api.Tests.TestSupport;

/// <summary>Terse factory helpers for building valid test entities.</summary>
public static class Make
{
    public static SurveyResponse Survey(
        IEnumerable<string> values,
        IEnumerable<string>? interests = null,
        TalentCategory? preferred = null,
        Guid? mentorId = null) => new()
    {
        Id = Guid.NewGuid(),
        MentorId = mentorId ?? Guid.NewGuid(),
        Values = values.ToList(),
        Interests = (interests ?? Enumerable.Empty<string>()).ToList(),
        PreferredTalentCategory = preferred
    };

    public static MenteeProfile Profile(
        string displayName,
        IEnumerable<string> tags,
        ProfileStatus status = ProfileStatus.Approved,
        TalentCategory category = TalentCategory.Student,
        Guid? careManagerId = null,
        DateTimeOffset? createdAt = null) => new()
    {
        Id = Guid.NewGuid(),
        DisplayName = displayName,
        Age = 14,
        Region = "Test Region",
        TalentCategory = category,
        Story = "A test story.",
        Tags = tags.ToList(),
        SupportNeeded = SupportNeeded.Mentorship,
        Status = status,
        CreatedByCareManagerId = careManagerId ?? Guid.NewGuid(),
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow
    };

    public static Mentor Mentor(Guid? appUserId = null) => new()
    {
        Id = Guid.NewGuid(),
        AppUserId = appUserId ?? Guid.NewGuid(),
        Country = "US"
    };

    public static CareManager CareManager(bool verified = true, Guid? appUserId = null) => new()
    {
        Id = Guid.NewGuid(),
        AppUserId = appUserId ?? Guid.NewGuid(),
        Region = "Test Division",
        IsVerified = verified
    };

    public static Mentorship Mentorship(Guid mentorId, MenteeProfile profile) => new()
    {
        Id = Guid.NewGuid(),
        MentorId = mentorId,
        MenteeProfileId = profile.Id,
        MenteeProfile = profile,
        RelationshipType = RelationshipType.Guidance,
        MeetingCadence = MeetingCadence.Monthly,
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        Status = MentorshipStatus.Active
    };
}
