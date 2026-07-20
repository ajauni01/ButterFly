using Butterfly.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Butterfly.Mobile.Extensions;

public static class HttpClientExtensions
{
    public static IHttpClientBuilder AddButterflyAccessToken(this IHttpClientBuilder builder) =>
        builder.AddHttpMessageHandler<AuthHeaderHandler>();
}
