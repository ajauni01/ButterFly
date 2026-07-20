using Butterfly.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Butterfly.Mobile.Extensions;

/// <summary>Refit/HttpClient wiring helpers for the Butterfly API client.</summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Attaches <see cref="AuthHeaderHandler"/> to the client pipeline so every request carries the
    /// Entra bearer token (acquired silently, with MSAL handling refresh).
    /// </summary>
    public static IHttpClientBuilder AddButterflyAccessToken(this IHttpClientBuilder builder) =>
        builder.AddHttpMessageHandler<AuthHeaderHandler>();
}
