using System.Security.Claims;
using Butterfly.Api.Infrastructure;
using Butterfly.Shared.Enums;

namespace Butterfly.Api.Auth;

/// <summary>
/// Reads the authenticated principal from the current <see cref="HttpContext"/>. Tolerant of the
/// several claim shapes Entra External ID uses for the same concept (oid, email, name).
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal _principal;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _principal = accessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
    }

    public bool IsAuthenticated => _principal.Identity?.IsAuthenticated ?? false;

    public string EntraObjectId =>
        FirstNonEmpty(
            "http://schemas.microsoft.com/identity/claims/objectidentifier",
            "oid")
        ?? throw new ApiException(StatusCodes.Status401Unauthorized, "unauthenticated",
            "Token is missing the required object-id (oid) claim.");

    public string Email =>
        FirstNonEmpty(
            "email",
            "preferred_username",
            ClaimTypes.Email,
            ClaimTypes.Upn)
        ?? string.Empty;

    public string DisplayName =>
        FirstNonEmpty("name", ClaimTypes.Name) ?? Email;

    public UserRole Role
    {
        get
        {
            // Microsoft.Identity.Web maps the App Roles "roles" claim to the role claim type.
            var role = _principal.FindFirst(ClaimTypes.Role)?.Value
                       ?? _principal.FindFirst("roles")?.Value
                       ?? _principal.FindFirst("role")?.Value;

            return role switch
            {
                AppRoles.Mentor => UserRole.Mentor,
                AppRoles.CareManager => UserRole.CareManager,
                AppRoles.Admin => UserRole.Admin,
                _ => throw new ApiException(StatusCodes.Status403Forbidden, "no_app_role",
                    "The token carries no recognized Butterfly app role. Assign the user a Mentor, CareManager, or Admin app role in Entra.")
            };
        }
    }

    private string? FirstNonEmpty(params string[] claimTypes)
    {
        foreach (var type in claimTypes)
        {
            var value = _principal.FindFirst(type)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }
        return null;
    }
}
