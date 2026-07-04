namespace Butterfly.Shared.Enums;

/// <summary>
/// Application roles. Mirrors the Entra External ID App Roles delivered in the
/// token's <c>roles</c> claim — this enum is never the source of truth for authorization,
/// only a typed mirror of the role Entra assigned.
/// </summary>
public enum UserRole
{
    Mentor = 0,
    CareManager = 1,
    Admin = 2
}
