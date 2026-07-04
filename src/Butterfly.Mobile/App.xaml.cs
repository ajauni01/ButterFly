using Butterfly.Mobile.Views;

namespace Butterfly.Mobile;

public partial class App : Application
{
    public App(IServiceProvider services)
    {
        InitializeComponent();

        // Start at sign-in. After authentication, AppNavigator swaps the root page to the
        // dashboard matching the user's role claim.
        MainPage = new NavigationPage(services.GetRequiredService<LoginPage>());
    }
}
