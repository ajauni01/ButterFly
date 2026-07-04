using Butterfly.Data.Entities;
using Butterfly.Shared.Dtos;

namespace Butterfly.Api.Services;

/// <summary>
/// Entity → DTO projections. The API returns Shared DTOs only, never EF entities, so mentee
/// safeguarding fields and internal navigation properties never leak over the wire by accident.
/// </summary>
public static class Mappings
{
    public static MenteeProfileDto ToDto(this MenteeProfile p) => new()
    {
        Id = p.Id,
        DisplayName = p.DisplayName,
        Age = p.Age,
        Region = p.Region,
        TalentCategory = p.TalentCategory,
        Story = p.Story,
        PhotoUrl = p.PhotoUrl,
        Tags = p.Tags.ToList(),
        SupportNeeded = p.SupportNeeded,
        MonthlyNeedBDT = p.MonthlyNeedBDT,
        ProfileStatus = p.Status,
        CreatedByCareManagerId = p.CreatedByCareManagerId,
        ApprovedByAdminId = p.ApprovedByAdminId,
        CreatedAt = p.CreatedAt
    };

    public static MentorshipDto ToDto(this Mentorship m) => new()
    {
        Id = m.Id,
        MentorId = m.MentorId,
        MenteeProfileId = m.MenteeProfileId,
        MenteeDisplayName = m.MenteeProfile?.DisplayName ?? string.Empty,
        RelationshipType = m.RelationshipType,
        MonthlyAmountUSD = m.MonthlyAmountUSD,
        MeetingCadence = m.MeetingCadence,
        StartDate = m.StartDate,
        Status = m.Status,
        CreatedAt = m.CreatedAt
    };

    public static ImpactUpdateDto ToDto(this ImpactUpdate u) => new()
    {
        Id = u.Id,
        MentorshipId = u.MentorshipId,
        CareManagerId = u.CareManagerId,
        WeekOf = u.WeekOf,
        UpdateType = u.UpdateType,
        SpendDescription = u.SpendDescription,
        AmountSpentBDT = u.AmountSpentBDT,
        SessionSummary = u.SessionSummary,
        ImpactNote = u.ImpactNote,
        PhotoUrl = u.PhotoUrl,
        CreatedAt = u.CreatedAt
    };

    public static PaymentDto ToDto(this Payment p) => new()
    {
        Id = p.Id,
        MentorshipId = p.MentorshipId,
        AmountUSD = p.AmountUSD,
        Method = p.Method,
        ExternalRef = p.ExternalRef,
        Status = p.Status,
        PaidAt = p.PaidAt,
        CreatedAt = p.CreatedAt
    };

    public static SurveyResponseDto ToDto(this SurveyResponse s) => new()
    {
        Id = s.Id,
        Values = s.Values.ToList(),
        Interests = s.Interests.ToList(),
        PreferredTalentCategory = s.PreferredTalentCategory,
        CreatedAt = s.CreatedAt
    };

    public static CareManagerDto ToDto(this CareManager c) => new()
    {
        Id = c.Id,
        DisplayName = c.AppUser?.DisplayName ?? string.Empty,
        Region = c.Region,
        IsVerified = c.IsVerified,
        CreatedAt = c.CreatedAt
    };
}
