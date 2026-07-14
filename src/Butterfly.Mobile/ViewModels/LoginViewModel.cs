using Butterfly.Mobile.Services;
using CommunityToolkit.Mvvm.Input;

namespace Butterfly.Mobile.ViewModels;

public sealed partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthenticationService _auth;
    private readonly IAppNavigator _navigator;

    public LoginViewModel(IAuthenticationService auth, IAppNavigator navigator)
    {
        _auth = auth;
        _navigator = navigator;
        Title = "Butterfly";
    }

    [RelayCommand]
    private Task SignInWithGoogleAsync() => SignInThroughUserFlowAsync();

    [RelayCommand]
    private Task ContinueWithEmailAsync() => SignInThroughUserFlowAsync();

    private async Task SignInThroughUserFlowAsync()
    {
        await RunAsync(async () =>
        {
            var role = await _auth.SignInAsync();
            _navigator.ShowRoleHome(role);
        });
    }
}
