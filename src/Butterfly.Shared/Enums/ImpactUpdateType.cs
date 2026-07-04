namespace Butterfly.Shared.Enums;

/// <summary>
/// The kind of weekly impact update a care manager logs.
/// <see cref="Spend"/> records how financial support was used; <see cref="Session"/> records an
/// overseen mentor↔mentee meeting; <see cref="Progress"/> is a general progress note.
/// </summary>
public enum ImpactUpdateType
{
    Spend = 0,
    Session = 1,
    Progress = 2
}
