using Butterfly.Mobile.ViewModels;

namespace Butterfly.Mobile.Views;

public partial class AdminPage : ContentPage
{
    private readonly AdminViewModel _vm;

    public AdminPage(AdminViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
