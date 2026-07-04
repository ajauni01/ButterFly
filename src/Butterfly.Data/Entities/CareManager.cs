namespace Butterfly.Data.Entities;

/// <summary>
/// A field care manager in Bangladesh — the trusted middleman who creates mentee profiles and
/// logs impact updates. 1:1 with an <see cref="AppUser"/> whose role is CareManager.
/// </summary>
public class CareManager : AuditableEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = null!;

    public string Region { get; set; } = string.Empty;

    /// <summary>Admin-set. Only verified care managers are trusted as middlemen.</summary>
    public bool IsVerified { get; set; }

    public ICollection<MenteeProfile> ManagedProfiles { get; set; } = new List<MenteeProfile>();
    public ICollection<ImpactUpdate> ImpactUpdates { get; set; } = new List<ImpactUpdate>();
}
