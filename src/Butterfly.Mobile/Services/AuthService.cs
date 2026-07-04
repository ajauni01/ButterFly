using System.Text.Json;
using Butterfly.Mobile.Configuration;
using Butterfly.Shared.Enums;
using Microsoft.Identity.Client;

namespace Butterfly.Mobile.Services;

/// <summary>
/// MSAL-based auth. The access token is never issued here — Entra External ID issues it; MSAL
/// caches it. The MSAL token cache is persisted to <see cref="SecureStorage"/> so sessions survive
/// app restarts. The role is read from the token's <c>roles</c> claim.
/// </summary>
public sealed class AuthService : IAuthService
{
    private const string CacheKey = "butterfly_msal_cache";

    private readonly IPublicClientApplication _pca;

    public AuthService()
    {
        var builder = PublicClientApplicationBuilder
            .Create(ButterflyConfig.ClientId)
            .WithAuthority(ButterflyConfig.Authority, "common", validateAuthority: false)
            .WithRedirectUri(ButterflyConfig.RedirectUri);

#if ANDROID
        builder = builder.WithParentActivityOrWindow(() => Platform.CurrentActivity);
#endif

        _pca = builder.Build();
        RegisterCache(_pca.UserTokenCache);
    }

    public async Task<UserRole> SignInAsync(CancellationToken ct = default)
    {
        AuthenticationResult result;
        try
        {
            var account = (await _pca.GetAccountsAsync()).FirstOrDefault();
            result = await _pca.AcquireTokenSilent(ButterflyConfig.Scopes, account).ExecuteAsync(ct);
        }
        catch (MsalUiRequiredException)
        {
            result = await _pca.AcquireTokenInteractive(ButterflyConfig.Scopes).ExecuteAsync(ct);
        }

        return ReadRole(result);
    }

    public async Task<string?> GetAccessTokenSilentAsync(CancellationToken ct = default)
    {
        var account = (await _pca.GetAccountsAsync()).FirstOrDefault();
        if (account is null)
            return null;

        try
        {
            var result = await _pca.AcquireTokenSilent(ButterflyConfig.Scopes, account).ExecuteAsync(ct);
            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            return null; // caller should route back to sign-in
        }
    }

    public async Task<bool> HasCachedAccountAsync() =>
        (await _pca.GetAccountsAsync()).Any();

    public async Task SignOutAsync()
    {
        foreach (var account in await _pca.GetAccountsAsync())
            await _pca.RemoveAsync(account);
        SecureStorage.Default.Remove(CacheKey);
    }

    private static UserRole ReadRole(AuthenticationResult result)
    {
        // Prefer the roles claim on the access token; fall back to the id token's claims.
        var role = result.ClaimsPrincipal?.FindFirst("roles")?.Value
                   ?? ReadRoleFromJwt(result.AccessToken);

        return role switch
        {
            "Mentor" => UserRole.Mentor,
            "CareManager" => UserRole.CareManager,
            "Admin" => UserRole.Admin,
            _ => throw new InvalidOperationException(
                "Your account has no Butterfly app role assigned. Ask an admin to assign Mentor, CareManager, or Admin in Entra.")
        };
    }

    private static string? ReadRoleFromJwt(string jwt)
    {
        try
        {
            // Decode the JWT payload (second segment) without a validation library — MSAL already
            // validated the token; we only need to read the roles claim.
            var parts = jwt.Split('.');
            if (parts.Length < 2) return null;

            var payload = parts[1];
            var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=')
                .Replace('-', '+').Replace('_', '/');
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("roles", out var roles))
                return null;

            return roles.ValueKind == JsonValueKind.Array
                ? roles.EnumerateArray().FirstOrDefault().GetString()
                : roles.GetString();
        }
        catch
        {
            return null;
        }
    }

    // ---- MSAL token cache persisted to SecureStorage ----
    private static void RegisterCache(ITokenCache cache)
    {
        cache.SetBeforeAccessAsync(async args =>
        {
            var data = await SecureStorage.Default.GetAsync(CacheKey);
            if (!string.IsNullOrEmpty(data))
                args.TokenCache.DeserializeMsalV3(Convert.FromBase64String(data));
        });

        cache.SetAfterAccessAsync(async args =>
        {
            if (args.HasStateChanged)
            {
                var bytes = args.TokenCache.SerializeMsalV3();
                await SecureStorage.Default.SetAsync(CacheKey, Convert.ToBase64String(bytes));
            }
        });
    }
}
