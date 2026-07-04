using Butterfly.Api.Auth;
using Butterfly.Api.Infrastructure;
using Butterfly.Api.Services;
using Butterfly.Data;
using Butterfly.Shared.Dtos;
using Butterfly.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Butterfly.Api.Controllers;

/// <summary>
/// Single mentee profile lookup with role-scoped visibility:
/// Mentors see only Approved profiles; Care Managers see only their own; Admins see all.
/// </summary>
[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class ProfilesController : ControllerBase
{
    private readonly ButterflyDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IUserProvisioningService _users;

    public ProfilesController(ButterflyDbContext db, ICurrentUser currentUser, IUserProvisioningService users)
    {
        _db = db;
        _currentUser = currentUser;
        _users = users;
    }

    [HttpGet("api/profiles/{id:guid}")]
    [ProducesResponseType(typeof(MenteeProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MenteeProfileDto>> GetProfile(Guid id, CancellationToken ct)
    {
        var profile = await _db.MenteeProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (profile is null)
            throw ApiException.NotFound("Mentee profile not found.");

        switch (_currentUser.Role)
        {
            case UserRole.Admin:
                break; // sees all

            case UserRole.Mentor:
                // SAFEGUARDING: mentors never see a non-approved profile — report 404, not 403,
                // so a pending profile's existence isn't disclosed.
                if (profile.Status != ProfileStatus.Approved)
                    throw ApiException.NotFound("Mentee profile not found.");
                break;

            case UserRole.CareManager:
                var careManager = await _users.GetCurrentCareManagerAsync(ct);
                if (profile.CreatedByCareManagerId != careManager.Id)
                    throw ApiException.NotFound("Mentee profile not found.");
                break;

            default:
                throw ApiException.Forbidden("Your role cannot view mentee profiles.");
        }

        return Ok(profile.ToDto());
    }
}
