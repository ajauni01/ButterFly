using Butterfly.Shared.Enums;

namespace Butterfly.Data.Entities;

/// <summary>
/// App-side profile for an authenticated principal, keyed by the Entra <c>oid</c> claim.
/// NO CREDENTIALS are stored here — Entra External ID owns authentication, passwords, and MFA.
/// Upserted from token claims on the first authenticated call (see <c>GET /api/me</c>).
/// </summary>
public class AppUser : AuditableEntity
{
    /// <summary>The Entra Object ID (<c>oid</c> claim). Unique, indexed — the join key to the identity provider.</summary>
    public string EntraObjectId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Mirrors the Entra App Role from the token's <c>roles</c> claim.</summary>
    public UserRole Role { get; set; }

    // Optional 1:1 role extensions, populated per role.
    public Mentor? Mentor { get; set; }
    public CareManager? CareManager { get; set; }
}
