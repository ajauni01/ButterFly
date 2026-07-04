namespace Butterfly.Api.Auth;

/// <summary>
/// Canonical role names. These MUST match the <c>value</c> of the App Roles defined in the API's
/// Entra External ID app registration — they arrive verbatim in the token's <c>roles</c> claim and
/// are what <c>[Authorize(Roles = ...)]</c> checks against.
/// </summary>
public static class AppRoles
{
    public const string Mentor = "Mentor";
    public const string CareManager = "CareManager";
    public const string Admin = "Admin";
}
