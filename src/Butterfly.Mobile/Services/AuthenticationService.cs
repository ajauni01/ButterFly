using System.Text.Json;
using Butterfly.Mobile.Configuration;
using Butterfly.Shared.Enums;
using Microsoft.Identity.Client;

namespace Butterfly.Mobile.Services;

/// <summary>
/// MSAL-based auth. Entra External ID owns the Google/email/password user flow and issues tokens;
/// the app only requests scopes and reads the resulting role claim.
/// </summary>
public sealed class AuthenticationService : IAuthService
{
    private readonly Lazy<IPublicClientApplication> _pca;

    public AuthenticationService()
    {
        _pca = new Lazy<IPublicClientApplication>(CreatePublicClientApplication);
    }

    public async Task<UserRole> SignInAsync(CancellationToken ct = default)
    {
        AuthenticationResult result;
        var account = (await _pca.Value.GetAccountsAsync()).FirstOrDefault();

        if (account is not null)
        {
            try
            {
                result = await _pca.Value.AcquireTokenSilent(ButterflyConfig.Scopes, account).ExecuteAsync(ct);
                return ReadRole(result);
            }
            catch (MsalUiRequiredException)
            {
                // Continue to the hosted user flow.
            }
        }

        try
        {
            result = await _pca.Value
                .AcquireTokenInteractive(ButterflyConfig.Scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync(ct);
        }
        catch (MsalClientException ex) when (ex.ErrorCode == MsalError.AuthenticationCanceledError)
        {
            throw new OperationCanceledException("Sign-in was cancelled.", ex, ct);
        }

        return ReadRole(result);
    }

    public async Task<string?> GetAccessTokenSilentAsync(CancellationToken ct = default)
    {
        var account = (await _pca.Value.GetAccountsAsync()).FirstOrDefault();
        if (account is null)
            return null;

        try
        {
            var result = await _pca.Value.AcquireTokenSilent(ButterflyConfig.Scopes, account).ExecuteAsync(ct);
            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            return null;
        }
        catch (MsalClientException ex) when (ex.ErrorCode == MsalError.AuthenticationCanceledError)
        {
            return null;
        }
    }

    public async Task<bool> HasCachedAccountAsync() =>
        (await _pca.Value.GetAccountsAsync()).Any();

    public async Task SignOutAsync()
    {
        foreach (var account in await _pca.Value.GetAccountsAsync())
            await _pca.Value.RemoveAsync(account);
    }

    private static IPublicClientApplication CreatePublicClientApplication()
    {
        var builder = PublicClientApplicationBuilder
            .Create(ButterflyConfig.ClientId)
            .WithB2CAuthority(ButterflyConfig.Authority)
            .WithRedirectUri(ButterflyConfig.RedirectUri);

#if IOS
        // Use this app's shared keychain group so device builds don't require publisher-owned groups.
        builder = builder.WithIosKeychainSecurityGroup("org.butterfly.mobile");
#endif

#if ANDROID
        builder = builder.WithParentActivityOrWindow(() => Platform.CurrentActivity);
#endif

        return builder.Build();
    }

    private static UserRole ReadRole(AuthenticationResult result)
    {
        var role = ReadRoleFromJwt(result.AccessToken)
                   ?? result.ClaimsPrincipal?.FindFirst("roles")?.Value;

        return role switch
        {
            "Mentor" => UserRole.Mentor,
            "CareManager" => UserRole.CareManager,
            "Admin" => UserRole.Admin,
            null => UserRole.Mentor,
            _ => throw new InvalidOperationException(
                "Your account has an unsupported Butterfly app role. Ask an admin to assign Mentor, CareManager, or Admin in Entra.")
        };
    }

    private static string? ReadRoleFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2)
                return null;

            var payload = parts[1];
            var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=')
                .Replace('-', '+')
                .Replace('_', '/');
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
}
