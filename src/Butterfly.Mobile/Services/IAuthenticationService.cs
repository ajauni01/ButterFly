using Butterfly.Shared.Enums;

namespace Butterfly.Mobile.Services;

/// <summary>Sign-in and token acquisition against Entra External ID via MSAL.</summary>
public interface IAuthenticationService
{
    /// <summary>Interactive sign-in. Returns the signed-in user's role (from the token's roles claim).</summary>
    Task<UserRole> SignInAsync(CancellationToken ct = default);

    /// <summary>Acquire an access token silently (cached/refresh); null if the user must sign in again.</summary>
    Task<string?> GetAccessTokenSilentAsync(CancellationToken ct = default);

    /// <summary>Whether a cached account exists (used to skip the login screen on relaunch).</summary>
    Task<bool> HasCachedAccountAsync();

    Task SignOutAsync();
}
