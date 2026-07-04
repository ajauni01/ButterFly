using Butterfly.Mobile.Services;
using CommunityToolkit.Mvvm.Input;

namespace Butterfly.Mobile.ViewModels;

public sealed partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IAppNavigator _navigator;

    public LoginViewModel(IAuthService auth, IAppNavigator navigator)
    {
        _auth = auth;
        _navigator = navigator;
        Title = "Butterfly";
    }

    [RelayCommand]
    private async Task SignInAsync()
    {
        await RunAsync(async () =>
        {
            var role = await _auth.SignInAsync();
            _navigator.ShowRoleHome(role);
        });
    }
}
