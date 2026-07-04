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

/// <summary>Mentor-only endpoints: survey, matches, mentorships, and impact feeds they own.</summary>
[ApiController]
[Authorize(Roles = AppRoles.Mentor)]
[Produces("application/json")]
public sealed class MentorsController : ControllerBase
{
    private readonly ButterflyDbContext _db;
    private readonly IUserProvisioningService _users;
    private readonly IMatchingService _matching;

    public MentorsController(ButterflyDbContext db, IUserProvisioningService users, IMatchingService matching)
    {
        _db = db;
        _users = users;
        _matching = matching;
    }

    /// <summary>Submit or replace the current mentor's values/interests survey.</summary>
    [HttpPost("api/mentors/survey")]
    [ProducesResponseType(typeof(SurveyResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SurveyResponseDto>> SubmitSurvey([FromBody] SurveyRequestDto request, CancellationToken ct)
    {
        var mentor = await _users.GetCurrentMentorAsync(ct);

        var survey = await _db.SurveyResponses.FirstOrDefaultAsync(s => s.MentorId == mentor.Id, ct);
        if (survey is null)
        {
            survey = new SurveyResponse { MentorId = mentor.Id };
            _db.SurveyResponses.Add(survey);
        }

        survey.Values = request.Values.ToList();
        survey.Interests = request.Interests.ToList();
        survey.PreferredTalentCategory = request.PreferredTalentCategory;

        await _db.SaveChangesAsync(ct);
        return Ok(survey.ToDto());
    }

    /// <summary>Tag-matched Approved mentee profiles for the current mentor, best match first.</summary>
    [HttpGet("api/mentors/matches")]
    [ProducesResponseType(typeof(IReadOnlyList<MenteeMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MenteeMatchDto>>> GetMatches(CancellationToken ct)
    {
        var mentor = await _users.GetCurrentMentorAsync(ct);
        var matches = await _matching.GetMatchesForMentorAsync(mentor.Id, ct);
        return Ok(matches);
    }

    /// <summary>Create a mentorship with an Approved mentee. Monthly amount required iff financial.</summary>
    [HttpPost("api/mentorships")]
    [ProducesResponseType(typeof(MentorshipDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MentorshipDto>> CreateMentorship([FromBody] CreateMentorshipRequestDto request, CancellationToken ct)
    {
        var mentor = await _users.GetCurrentMentorAsync(ct);

        var financial = request.RelationshipType is RelationshipType.Financial or RelationshipType.Both;
        if (financial && request.MonthlyAmountUSD is null)
            throw ApiException.BadRequest("A monthly amount is required for Financial or Both relationships.");
        if (!financial && request.MonthlyAmountUSD is not null)
            throw ApiException.BadRequest("A monthly amount must not be set for a Guidance-only relationship.");

        // SAFEGUARDING: a mentor may only ever start a mentorship with an APPROVED profile.
        var profile = await _db.MenteeProfiles
            .FirstOrDefaultAsync(p => p.Id == request.MenteeProfileId && p.Status == ProfileStatus.Approved, ct)
            ?? throw ApiException.NotFound("No approved mentee profile with that id was found.");

        if (await _db.Mentorships.AnyAsync(m => m.MentorId == mentor.Id && m.MenteeProfileId == profile.Id, ct))
            throw ApiException.Conflict("You already have a mentorship with this mentee.");

        var mentorship = new Mentorship
        {
            MentorId = mentor.Id,
            MenteeProfileId = profile.Id,
            RelationshipType = request.RelationshipType,
            MonthlyAmountUSD = request.MonthlyAmountUSD,
            MeetingCadence = request.MeetingCadence,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = MentorshipStatus.Active
        };
        _db.Mentorships.Add(mentorship);
        await _db.SaveChangesAsync(ct);

        mentorship.MenteeProfile = profile;
        return CreatedAtAction(nameof(GetImpact), new { id = mentorship.Id }, mentorship.ToDto());
    }

    /// <summary>The current mentor's mentorships.</summary>
    [HttpGet("api/mentorships/mine")]
    [ProducesResponseType(typeof(IReadOnlyList<MentorshipDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MentorshipDto>>> GetMine(CancellationToken ct)
    {
        var mentor = await _users.GetCurrentMentorAsync(ct);
        var mentorships = await _db.Mentorships
            .AsNoTracking()
            .Include(m => m.MenteeProfile)
            .Where(m => m.MentorId == mentor.Id)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        return Ok(mentorships.Select(m => m.ToDto()).ToList());
    }

    /// <summary>Impact feed for one mentorship. OWNERSHIP: the mentorship must belong to the caller.</summary>
    [HttpGet("api/mentorships/{id:guid}/impact")]
    [ProducesResponseType(typeof(IReadOnlyList<ImpactUpdateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ImpactUpdateDto>>> GetImpact(Guid id, CancellationToken ct)
    {
        var mentor = await _users.GetCurrentMentorAsync(ct);

        var mentorship = await _db.Mentorships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw ApiException.NotFound("Mentorship not found.");

        if (mentorship.MentorId != mentor.Id)
            throw ApiException.Forbidden("This mentorship does not belong to you.");

        var updates = await _db.ImpactUpdates
            .AsNoTracking()
            .Where(u => u.MentorshipId == id)
            .OrderByDescending(u => u.WeekOf)
            .ToListAsync(ct);

        return Ok(updates.Select(u => u.ToDto()).ToList());
    }
}
