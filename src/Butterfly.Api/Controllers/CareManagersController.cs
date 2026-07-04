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

/// <summary>Care-manager-only endpoints: create mentee profiles and log weekly impact updates.</summary>
[ApiController]
[Authorize(Roles = AppRoles.CareManager)]
[Produces("application/json")]
public sealed class CareManagersController : ControllerBase
{
    private readonly ButterflyDbContext _db;
    private readonly IUserProvisioningService _users;

    public CareManagersController(ButterflyDbContext db, IUserProvisioningService users)
    {
        _db = db;
        _users = users;
    }

    /// <summary>
    /// Create a mentee profile. Always saved <c>Pending</c> — an Admin must approve it before any
    /// mentor can see it. Financial/Both need a monthly amount; Mentorship-only must not have one.
    /// </summary>
    [HttpPost("api/caremanagers/profiles")]
    [ProducesResponseType(typeof(MenteeProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MenteeProfileDto>> CreateProfile([FromBody] CreateMenteeProfileRequestDto request, CancellationToken ct)
    {
        var careManager = await _users.GetCurrentCareManagerAsync(ct);

        var financial = request.SupportNeeded is SupportNeeded.Financial or SupportNeeded.Both;
        if (financial && request.MonthlyNeedBDT is null)
            throw ApiException.BadRequest("A monthly need (BDT) is required when financial support is needed.");
        if (!financial && request.MonthlyNeedBDT is not null)
            throw ApiException.BadRequest("A monthly need (BDT) must not be set for a mentorship-only profile.");

        var profile = new MenteeProfile
        {
            DisplayName = request.DisplayName,
            Age = request.Age,
            Region = request.Region,
            TalentCategory = request.TalentCategory,
            Story = request.Story,
            PhotoUrl = request.PhotoUrl,
            Tags = request.Tags.ToList(),
            SupportNeeded = request.SupportNeeded,
            MonthlyNeedBDT = request.MonthlyNeedBDT,
            Status = ProfileStatus.Pending, // never visible to a mentor until Admin approval
            CreatedByCareManagerId = careManager.Id
        };
        _db.MenteeProfiles.Add(profile);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetMyProfiles), null, profile.ToDto());
    }

    /// <summary>The current care manager's own profiles and their statuses.</summary>
    [HttpGet("api/caremanagers/profiles/mine")]
    [ProducesResponseType(typeof(IReadOnlyList<MenteeProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MenteeProfileDto>>> GetMyProfiles(CancellationToken ct)
    {
        var careManager = await _users.GetCurrentCareManagerAsync(ct);
        var profiles = await _db.MenteeProfiles
            .AsNoTracking()
            .Where(p => p.CreatedByCareManagerId == careManager.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return Ok(profiles.Select(p => p.ToDto()).ToList());
    }

    /// <summary>
    /// Log a weekly impact update. OWNERSHIP: only for a mentorship whose mentee profile this care
    /// manager created. Required fields depend on the update type.
    /// </summary>
    [HttpPost("api/mentorships/{id:guid}/impact")]
    [ProducesResponseType(typeof(ImpactUpdateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImpactUpdateDto>> LogImpact(Guid id, [FromBody] CreateImpactUpdateRequestDto request, CancellationToken ct)
    {
        var careManager = await _users.GetCurrentCareManagerAsync(ct);

        var mentorship = await _db.Mentorships
            .Include(m => m.MenteeProfile)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw ApiException.NotFound("Mentorship not found.");

        // OWNERSHIP: the mentee in this mentorship must be one this care manager manages.
        if (mentorship.MenteeProfile.CreatedByCareManagerId != careManager.Id)
            throw ApiException.Forbidden("You do not manage the mentee in this mentorship.");

        ValidateImpactShape(request);

        var update = new ImpactUpdate
        {
            MentorshipId = mentorship.Id,
            CareManagerId = careManager.Id,
            WeekOf = request.WeekOf,
            UpdateType = request.UpdateType,
            SpendDescription = request.SpendDescription,
            AmountSpentBDT = request.AmountSpentBDT,
            SessionSummary = request.SessionSummary,
            ImpactNote = request.ImpactNote,
            PhotoUrl = request.PhotoUrl
        };
        _db.ImpactUpdates.Add(update);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetMyProfiles), null, update.ToDto());
    }

    private static void ValidateImpactShape(CreateImpactUpdateRequestDto r)
    {
        switch (r.UpdateType)
        {
            case ImpactUpdateType.Spend when r.AmountSpentBDT is null:
                throw ApiException.BadRequest("A Spend update requires AmountSpentBDT.");
            case ImpactUpdateType.Session when string.IsNullOrWhiteSpace(r.SessionSummary):
                throw ApiException.BadRequest("A Session update requires a SessionSummary.");
        }
    }
}
