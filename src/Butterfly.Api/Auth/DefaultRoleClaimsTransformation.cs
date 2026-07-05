using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Butterfly.Api.Auth;

/// <summary>
/// Self-service default role. Entra External ID lets anyone sign up (e.g. with a Google account),
/// but self-signup does NOT assign an Entra App Role — so such a token carries no <c>roles</c> claim
/// and would be rejected by every <c>[Authorize(Roles = ...)]</c> endpoint.
///
/// This transformation runs on each authenticated request: if the principal has none of the
/// Butterfly roles, it grants <see cref="AppRoles.Mentor"/> — the public-facing persona. CareManager
/// and Admin stay invite-only: they are assigned explicitly in Entra and always win when present.
/// </summary>
public sealed class DefaultRoleClaimsTransformation : IClaimsTransformation
{
    private static readonly string[] KnownRoles = { AppRoles.Mentor, AppRoles.CareManager, AppRoles.Admin };

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        // Already carries a recognized Butterfly role (from an assigned Entra App Role)? Leave it.
        var hasKnownRole = KnownRoles.Any(principal.IsInRole)
            || principal.FindAll("roles").Any(c => KnownRoles.Contains(c.Value));
        if (hasKnownRole)
            return Task.FromResult(principal);

        // Grant the default role using the identity's role claim type so [Authorize(Roles="Mentor")]
        // and ICurrentUser both see it. Idempotent: the guard above prevents a second add on re-run.
        identity.AddClaim(new Claim(identity.RoleClaimType, AppRoles.Mentor));
        return Task.FromResult(principal);
    }
}
