using Butterfly.Mobile.Configuration;
using Butterfly.Mobile.Extensions;
using Butterfly.Mobile.Services;
using Butterfly.Mobile.ViewModels;
using Butterfly.Mobile.Views;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace Butterfly.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ---- Auth + navigation ----
        builder.Services.AddSingleton<AuthenticationService>();
        builder.Services.AddSingleton<IAuthenticationService>(sp => sp.GetRequiredService<AuthenticationService>());
        builder.Services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<AuthenticationService>());
        builder.Services.AddSingleton<IAppNavigator, AppNavigator>();
        builder.Services.AddTransient<AuthHeaderHandler>();

        // ---- Typed API client: Refit + bearer handler + Polly transient-retry ----
        builder.Services
            .AddRefitClient<IButterflyApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(ButterflyConfig.ApiBaseUrl))
            .AddButterflyAccessToken()
            .AddPolicyHandler(RetryPolicy())
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(() => DevHandler())
#endif
            ;
        builder.Services.AddTransient<IApiClient, ApiClient>();

        // ---- View models ----
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<MentorViewModel>();
        builder.Services.AddTransient<SurveyViewModel>();
        builder.Services.AddTransient<CareManagerViewModel>();
        builder.Services.AddTransient<AdminViewModel>();

        // ---- Pages ----
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MentorPage>();
        builder.Services.AddTransient<SurveyPage>();
        builder.Services.AddTransient<CareManagerPage>();
        builder.Services.AddTransient<AdminPage>();

        builder.Services.AddSingleton<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    // Retry transient failures (5xx, 408, network) with exponential backoff.
    private static IAsyncPolicy<HttpResponseMessage> RetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(300 * Math.Pow(2, attempt)));

#if DEBUG
    // DEV ONLY: trust the ASP.NET Core self-signed dev certificate so the emulator/simulator can
    // reach https://localhost (or 10.0.2.2). Never ship this — production uses a real certificate.
    private static HttpMessageHandler DevHandler()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        return handler;
    }
#endif
}
