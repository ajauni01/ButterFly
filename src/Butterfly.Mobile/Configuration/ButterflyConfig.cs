namespace Butterfly.Mobile.Configuration;

/// <summary>
/// Client configuration for Microsoft Entra External ID (CIAM).
/// These are NOT secrets — a native public client has no client secret; MSAL uses auth-code + PKCE.
/// The only value that must match server-side is <see cref="ApiScope"/>.
/// </summary>
public static class ButterflyConfig
{
    public const string TenantSubdomain = "butterflyciam";
    public const string TenantId = "99ab3d07-4cdd-4307-b06c-50210e64a56c";
    public const string UserFlow = "B2C_1_signupsignin";

    /// <summary>The CIAM user-flow authority. Entra handles Google and email/password here.</summary>
    public static string Authority => $"https://{TenantSubdomain}.ciamlogin.com/{TenantId}/{UserFlow}/v2.0";

    /// <summary>The Butterfly.Client (public/mobile) app registration's Application (client) ID.</summary>
    public const string ClientId = "578742b9-68d1-4679-a658-fa6a1d8e528c";

    /// <summary>
    /// The delegated scope exposed by the API app registration, fully qualified:
    /// api://&lt;api-client-id&gt;/access_as_user.
    /// </summary>
    public const string ApiScope = "api://2b22acf8-4674-4bef-b041-7e135c2edd67/access_as_user";

    public static string[] Scopes => new[] { ApiScope, "openid", "profile" };

    /// <summary>
    /// Base URL of the running API. Android emulator reaches the host machine at 10.0.2.2.
    /// iOS simulator and MacCatalyst reach it at localhost. Adjust per your run target / deployment.
    /// </summary>
#if ANDROID
    public const string ApiBaseUrl = "https://10.0.2.2:5001";
#else
    public const string ApiBaseUrl = "https://localhost:5001";
#endif

    /// <summary>
    /// MSAL redirect URI for the native client. Must be registered on Butterfly.Client as a
    /// mobile/desktop redirect URI. The msal{clientId}:// scheme is the platform default.
    /// </summary>
    public static string RedirectUri => $"msal{ClientId}://auth";
}
