using Butterfly.Mobile.ViewModels;

namespace Butterfly.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
