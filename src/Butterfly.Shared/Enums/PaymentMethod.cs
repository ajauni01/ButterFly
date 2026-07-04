namespace Butterfly.Shared.Enums;

/// <summary>
/// How a payment was recorded. Pilot is record-only: <see cref="Manual"/> is the default.
/// <see cref="Stripe"/> exists so a future integration slots in without a schema change — NOT wired up yet.
/// </summary>
public enum PaymentMethod
{
    Manual = 0,
    Stripe = 1
}
