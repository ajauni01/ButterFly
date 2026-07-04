using Butterfly.Shared.Enums;

namespace Butterfly.Data.Entities;

/// <summary>
/// A mentor's values/interests survey. <see cref="Values"/> and <see cref="Interests"/> are stored
/// as JSON string lists (see ButterflyDbContext value-converter config) — no separate tag table.
/// One current response per mentor (replaced on re-submit).
/// </summary>
public class SurveyResponse : AuditableEntity
{
    public Guid MentorId { get; set; }
    public Mentor Mentor { get; set; } = null!;

    public List<string> Values { get; set; } = new();
    public List<string> Interests { get; set; } = new();

    public TalentCategory? PreferredTalentCategory { get; set; }
}
