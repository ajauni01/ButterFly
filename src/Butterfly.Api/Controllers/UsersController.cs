using Butterfly.Api.Services;
using Butterfly.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Butterfly.Api.Controllers;

/// <summary>
/// Current-user endpoints. Authentication (register/login/reset) is entirely Entra's job — this API
/// has no such endpoints; it only reconciles an app-side profile from the validated token.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserProvisioningService _provisioning;

    public UsersController(IUserProvisioningService provisioning)
    {
        _provisioning = provisioning;
    }

    /// <summary>
    /// Returns the current user's app profile. On the first authenticated call this upserts the
    /// <c>AppUser</c> from the token's oid/email/name claims and creates the matching Mentor or
    /// CareManager record based on the role claim.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserProfileDto>> GetMe(CancellationToken ct)
    {
        var profile = await _provisioning.GetOrProvisionCurrentUserAsync(ct);
        return Ok(profile);
    }
}
