using Butterfly.Shared.Enums;

namespace Butterfly.Mobile.Services;

/// <summary>Sign-in, token acquisition, refresh, and logout against Entra External ID via MSAL.</summary>
public interface IAuthenticationService
{
    /// <summary>Interactive sign-in. Returns the signed-in user's Butterfly role.</summary>
    Task<UserRole> SignInAsync(CancellationToken ct = default);

    /// <summary>Acquire an access token silently from MSAL's cache, refreshing when possible.</summary>
    Task<string?> GetAccessTokenSilentAsync(CancellationToken ct = default);

    /// <summary>Whether MSAL has a cached account for this app.</summary>
    Task<bool> HasCachedAccountAsync();

    /// <summary>Remove cached MSAL accounts and tokens for this app.</summary>
    Task SignOutAsync();
}
