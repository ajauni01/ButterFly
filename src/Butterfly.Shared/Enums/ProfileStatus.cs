namespace Butterfly.Shared.Enums;

/// <summary>
/// Lifecycle of a <c>MenteeProfile</c>. SAFEGUARDING: a profile is <see cref="Pending"/>
/// on creation and is NEVER visible to a Mentor until an Admin sets it <see cref="Approved"/>.
/// </summary>
public enum ProfileStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Archived = 3
}
