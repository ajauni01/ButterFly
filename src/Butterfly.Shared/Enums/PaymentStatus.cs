namespace Butterfly.Shared.Enums;

/// <summary>
/// State of a recorded payment. Pilot payments are entered manually and land as
/// <see cref="Recorded"/>; an Admin can mark them <see cref="Confirmed"/>. <see cref="Failed"/>
/// is reserved for a future live processor.
/// </summary>
public enum PaymentStatus
{
    Recorded = 0,
    Confirmed = 1,
    Failed = 2
}
