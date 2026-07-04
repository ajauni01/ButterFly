using Butterfly.Shared.Enums;

namespace Butterfly.Data.Entities;

/// <summary>
/// Record-only payment scaffold for the pilot. No live processor is wired up. The shape
/// (<see cref="Method"/>, <see cref="ExternalRef"/>, <see cref="Status"/>) is deliberately
/// Stripe-ready so a real integration slots in without a schema change. Only applies to
/// Financial/Both mentorships.
/// </summary>
public class Payment : AuditableEntity
{
    public Guid MentorshipId { get; set; }
    public Mentorship Mentorship { get; set; } = null!;

    public decimal AmountUSD { get; set; }

    public PaymentMethod Method { get; set; } = PaymentMethod.Manual;

    /// <summary>Future Stripe charge/payment-intent id. Null for manual pilot entries.</summary>
    public string? ExternalRef { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Recorded;

    public DateTimeOffset PaidAt { get; set; }
}
