using System.ComponentModel.DataAnnotations;
using Butterfly.Shared.Enums;

namespace Butterfly.Shared.Dtos;

/// <summary>
/// Manually record a payment against a mentorship (<c>POST /api/admin/payments</c>).
/// Record-only for the pilot — no live processor. Applies only to Financial/Both mentorships.
/// </summary>
public sealed record CreatePaymentRequestDto
{
    [Required]
    public required Guid MentorshipId { get; init; }

    [Range(1, 100_000)]
    public required decimal AmountUSD { get; init; }

    /// <summary>Defaults to Manual for the pilot. Stripe is reserved for a future integration.</summary>
    public PaymentMethod Method { get; init; } = PaymentMethod.Manual;

    /// <summary>Optional external reference (e.g. a future Stripe charge id).</summary>
    [StringLength(200)]
    public string? ExternalRef { get; init; }

    /// <summary>When the payment was actually made. Defaults to now if omitted server-side.</summary>
    public DateTimeOffset? PaidAt { get; init; }
}

/// <summary>A recorded payment.</summary>
public sealed record PaymentDto
{
    public required Guid Id { get; init; }
    public required Guid MentorshipId { get; init; }
    public required decimal AmountUSD { get; init; }
    public required PaymentMethod Method { get; init; }
    public string? ExternalRef { get; init; }
    public required PaymentStatus Status { get; init; }
    public required DateTimeOffset PaidAt { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
