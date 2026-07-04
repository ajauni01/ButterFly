using Butterfly.Shared.Enums;

namespace Butterfly.Data.Entities;

/// <summary>
/// A mentor↔mentee relationship. <see cref="MonthlyAmountUSD"/> is null for Guidance-only.
/// </summary>
public class Mentorship : AuditableEntity
{
    public Guid MentorId { get; set; }
    public Mentor Mentor { get; set; } = null!;

    public Guid MenteeProfileId { get; set; }
    public MenteeProfile MenteeProfile { get; set; } = null!;

    public RelationshipType RelationshipType { get; set; }

    /// <summary>Monthly commitment in USD. Null for Guidance-only relationships.</summary>
    public decimal? MonthlyAmountUSD { get; set; }

    public MeetingCadence MeetingCadence { get; set; }
    public DateOnly StartDate { get; set; }
    public MentorshipStatus Status { get; set; } = MentorshipStatus.Active;

    public ICollection<ImpactUpdate> ImpactUpdates { get; set; } = new List<ImpactUpdate>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
