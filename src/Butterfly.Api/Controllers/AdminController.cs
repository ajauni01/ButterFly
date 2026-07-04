using Butterfly.Api.Auth;
using Butterfly.Api.Infrastructure;
using Butterfly.Api.Services;
using Butterfly.Data;
using Butterfly.Data.Entities;
using Butterfly.Shared.Dtos;
using Butterfly.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Butterfly.Api.Controllers;

/// <summary>Admin-only endpoints: profile approval queue, care-manager verification, payment records.</summary>
[ApiController]
[Authorize(Roles = AppRoles.Admin)]
[Produces("application/json")]
public sealed class AdminController : ControllerBase
{
    private readonly ButterflyDbContext _db;
    private readonly IUserProvisioningService _users;

    public AdminController(ButterflyDbContext db, IUserProvisioningService users)
    {
        _db = db;
        _users = users;
    }

    /// <summary>All mentee profiles awaiting approval, oldest first.</summary>
    [HttpGet("api/admin/profiles/pending")]
    [ProducesResponseType(typeof(IReadOnlyList<MenteeProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MenteeProfileDto>>> GetPending(CancellationToken ct)
    {
        var pending = await _db.MenteeProfiles
            .AsNoTracking()
            .Where(p => p.Status == ProfileStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(ct);

        return Ok(pending.Select(p => p.ToDto()).ToList());
    }

    /// <summary>Approve a pending profile — records the approving admin and makes it mentor-visible.</summary>
    [HttpPost("api/admin/profiles/{id:guid}/approve")]
    [ProducesResponseType(typeof(MenteeProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenteeProfileDto>> Approve(Guid id, CancellationToken ct)
    {
        var admin = await _users.EnsureCurrentAppUserAsync(ct);
        var profile = await GetPendingProfile(id, ct);

        profile.Status = ProfileStatus.Approved;
        profile.ApprovedByAdminId = admin.Id;
        profile.RejectionReason = null;
        await _db.SaveChangesAsync(ct);

        return Ok(profile.ToDto());
    }

    /// <summary>Reject a pending profile with a reason (kept for the care manager).</summary>
    [HttpPost("api/admin/profiles/{id:guid}/reject")]
    [ProducesResponseType(typeof(MenteeProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenteeProfileDto>> Reject(Guid id, [FromBody] RejectProfileRequestDto request, CancellationToken ct)
    {
        var admin = await _users.EnsureCurrentAppUserAsync(ct);
        var profile = await GetPendingProfile(id, ct);

        profile.Status = ProfileStatus.Rejected;
        profile.ApprovedByAdminId = admin.Id;
        profile.RejectionReason = request.Reason;
        await _db.SaveChangesAsync(ct);

        return Ok(profile.ToDto());
    }

    /// <summary>Mark a care manager as verified (trusted middleman).</summary>
    [HttpPost("api/admin/caremanagers/{id:guid}/verify")]
    [ProducesResponseType(typeof(CareManagerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CareManagerDto>> VerifyCareManager(Guid id, CancellationToken ct)
    {
        var careManager = await _db.CareManagers
            .Include(c => c.AppUser)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw ApiException.NotFound("Care manager not found.");

        careManager.IsVerified = true;
        await _db.SaveChangesAsync(ct);

        return Ok(careManager.ToDto());
    }

    /// <summary>
    /// Manually record a payment against a mentorship (record-only pilot). Only valid for
    /// Financial/Both mentorships.
    /// </summary>
    [HttpPost("api/admin/payments")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> RecordPayment([FromBody] CreatePaymentRequestDto request, CancellationToken ct)
    {
        var mentorship = await _db.Mentorships.FirstOrDefaultAsync(m => m.Id == request.MentorshipId, ct)
            ?? throw ApiException.NotFound("Mentorship not found.");

        if (mentorship.RelationshipType is RelationshipType.Guidance)
            throw ApiException.BadRequest("Payments apply only to Financial or Both mentorships.");

        var payment = new Payment
        {
            MentorshipId = mentorship.Id,
            AmountUSD = request.AmountUSD,
            Method = request.Method,
            ExternalRef = request.ExternalRef,
            Status = PaymentStatus.Recorded,
            PaidAt = request.PaidAt ?? DateTimeOffset.UtcNow
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetPending), null, payment.ToDto());
    }

    private async Task<MenteeProfile> GetPendingProfile(Guid id, CancellationToken ct)
    {
        var profile = await _db.MenteeProfiles.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw ApiException.NotFound("Mentee profile not found.");

        if (profile.Status != ProfileStatus.Pending)
            throw ApiException.BadRequest($"Profile is not pending (current status: {profile.Status}).");

        return profile;
    }
}
