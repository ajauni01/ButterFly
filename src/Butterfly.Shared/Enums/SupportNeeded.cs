namespace Butterfly.Shared.Enums;

/// <summary>
/// What a mentee needs. Drives whether a money commitment applies:
/// a monthly need amount is only meaningful for <see cref="Financial"/> or <see cref="Both"/>.
/// </summary>
public enum SupportNeeded
{
    Financial = 0,
    Mentorship = 1,
    Both = 2
}
