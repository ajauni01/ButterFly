using System.ComponentModel.DataAnnotations;

namespace Butterfly.Shared.Dtos;

/// <summary>
/// Reject a pending mentee profile (<c>POST /api/admin/profiles/{id}/reject</c>).
/// A reason is required so the care manager knows what to fix.
/// </summary>
public sealed record RejectProfileRequestDto
{
    [Required, StringLength(1000, MinimumLength = 1)]
    public required string Reason { get; init; }
}
