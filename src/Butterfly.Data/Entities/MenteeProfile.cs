using Butterfly.Shared.Enums;

namespace Butterfly.Data.Entities;

/// <summary>
/// A mentee profile. SAFEGUARDING (enforced here, not just in the UI):
/// <list type="bullet">
/// <item><see cref="DisplayName"/> is a first name / pseudonym only — never a legal name.</item>
/// <item><see cref="Region"/> is village-level only — no address or GPS is modeled at all.</item>
/// <item><see cref="Status"/> starts <see cref="ProfileStatus.Pending"/> and must be Admin-approved
/// before any mentor query returns it.</item>
/// </list>
/// </summary>
public class MenteeProfile : AuditableEntity
{
    /// <summary>First name or pseudonym only.</summary>
    public string DisplayName { get; set; } = string.Empty;

    public int Age { get; set; }

    /// <summary>Region / village-level location only.</summary>
    public string Region { get; set; } = string.Empty;

    public TalentCategory TalentCategory { get; set; }
    public string Story { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }

    /// <summary>Matching tags, stored as a JSON string list.</summary>
    public List<string> Tags { get; set; } = new();

    public SupportNeeded SupportNeeded { get; set; }

    /// <summary>Monthly need in BDT. Null when guidance-only (SupportNeeded = Mentorship).</summary>
    public decimal? MonthlyNeedBDT { get; set; }

    public ProfileStatus Status { get; set; } = ProfileStatus.Pending;

    public Guid CreatedByCareManagerId { get; set; }
    public CareManager CreatedByCareManager { get; set; } = null!;

    /// <summary>The admin who approved (or last actioned) this profile; null while Pending.</summary>
    public Guid? ApprovedByAdminId { get; set; }
    public AppUser? ApprovedByAdmin { get; set; }

    /// <summary>Reason captured when an admin rejects the profile.</summary>
    public string? RejectionReason { get; set; }

    public ICollection<Mentorship> Mentorships { get; set; } = new List<Mentorship>();
}
