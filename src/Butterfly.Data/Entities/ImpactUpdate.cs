using Butterfly.Shared.Enums;

namespace Butterfly.Data.Entities;

/// <summary>
/// A care-manager-authored weekly impact update — the mediated channel through which impact reaches
/// the mentor. There is no direct mentor↔mentee messaging; this entity is the transparency mechanism.
/// Which optional fields are populated depends on <see cref="UpdateType"/>.
/// </summary>
public class ImpactUpdate : AuditableEntity
{
    public Guid MentorshipId { get; set; }
    public Mentorship Mentorship { get; set; } = null!;

    public Guid CareManagerId { get; set; }
    public CareManager CareManager { get; set; } = null!;

    public DateOnly WeekOf { get; set; }
    public ImpactUpdateType UpdateType { get; set; }

    /// <summary>For Spend updates: what the money was used for.</summary>
    public string? SpendDescription { get; set; }

    /// <summary>For Spend updates: amount spent in BDT.</summary>
    public decimal? AmountSpentBDT { get; set; }

    /// <summary>For Session updates: summary of an overseen mentor↔mentee meeting.</summary>
    public string? SessionSummary { get; set; }

    public string ImpactNote { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}
