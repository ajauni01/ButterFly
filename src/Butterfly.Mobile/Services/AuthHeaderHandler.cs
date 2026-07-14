using System.Net.Http.Headers;

namespace Butterfly.Mobile.Services;

/// <summary>
/// Attaches the Entra bearer token (acquired silently, with MSAL handling refresh) to every API
/// request. If no valid token is available the request proceeds unauthenticated and the API's 401
/// surfaces to the caller, which routes back to sign-in.
/// </summary>
public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly IAuthenticationService _auth;

    public AuthHeaderHandler(IAuthenticationService auth) => _auth = auth;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _auth.GetAccessTokenSilentAsync(cancellationToken);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
