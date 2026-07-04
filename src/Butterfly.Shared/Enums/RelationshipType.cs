namespace Butterfly.Shared.Enums;

/// <summary>
/// The nature of a mentor↔mentee mentorship. A monthly USD amount applies only to
/// <see cref="Financial"/> or <see cref="Both"/>; it is null for <see cref="Guidance"/>-only.
/// </summary>
public enum RelationshipType
{
    Financial = 0,
    Guidance = 1,
    Both = 2
}
