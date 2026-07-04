using Butterfly.Mobile.Views;
using Butterfly.Shared.Enums;

namespace Butterfly.Mobile.Services;

/// <summary>Resolves role-specific pages from DI and swaps the app's root page.</summary>
public sealed class AppNavigator : IAppNavigator
{
    private readonly IServiceProvider _services;

    public AppNavigator(IServiceProvider services) => _services = services;

    public void ShowLogin() =>
        SetRoot(_services.GetRequiredService<LoginPage>());

    public void ShowRoleHome(UserRole role)
    {
        Page home = role switch
        {
            UserRole.Mentor => Wrap("Mentor", _services.GetRequiredService<MentorPage>()),
            UserRole.CareManager => Wrap("Care Manager", _services.GetRequiredService<CareManagerPage>()),
            UserRole.Admin => Wrap("Admin", _services.GetRequiredService<AdminPage>()),
            _ => _services.GetRequiredService<LoginPage>()
        };
        SetRoot(home);
    }

    private static NavigationPage Wrap(string title, Page page)
    {
        page.Title = title;
        return new NavigationPage(page);
    }

    private static void SetRoot(Page page)
    {
        if (Application.Current is not null)
            Application.Current.MainPage = page;
    }
}
