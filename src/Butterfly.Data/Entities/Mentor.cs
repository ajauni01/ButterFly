namespace Butterfly.Data.Entities;

/// <summary>A US-based mentor. 1:1 with an <see cref="AppUser"/> whose role is Mentor.</summary>
public class Mentor : AuditableEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = null!;

    public string Country { get; set; } = "US";

    /// <summary>The mentor's latest values/interests survey (null until they complete it).</summary>
    public SurveyResponse? Survey { get; set; }

    public ICollection<Mentorship> Mentorships { get; set; } = new List<Mentorship>();
}
