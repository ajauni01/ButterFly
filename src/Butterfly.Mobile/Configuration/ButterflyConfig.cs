namespace Butterfly.Mobile.Configuration;

/// <summary>
/// Client configuration. Fill these in after the Entra tenant and API are set up (see README).
/// These are NOT secrets — a native public client has no client secret; MSAL uses auth-code + PKCE.
/// The only value that must match server-side is <see cref="ApiScope"/>.
/// </summary>
public static class ButterflyConfig
{
    /// <summary>Your External ID tenant subdomain, e.g. "butterflydev" for butterflydev.ciamlogin.com.</summary>
    public const string TenantSubdomain = "butterflydev";

    /// <summary>The CIAM authority. Built from the subdomain.</summary>
    public static string Authority => $"https://{TenantSubdomain}.ciamlogin.com/";

    /// <summary>The Butterfly.Client (public/mobile) app registration's Application (client) ID.</summary>
    public const string ClientId = "22222222-2222-2222-2222-222222222222";

    /// <summary>
    /// The delegated scope exposed by the API app registration, fully qualified:
    /// api://&lt;api-client-id&gt;/access_as_user.
    /// </summary>
    public const string ApiScope = "api://11111111-1111-1111-1111-111111111111/access_as_user";

    public static string[] Scopes => new[] { ApiScope };

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
