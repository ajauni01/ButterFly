using Butterfly.Mobile.ViewModels;

namespace Butterfly.Mobile.Views;

public partial class CareManagerPage : ContentPage
{
    private readonly CareManagerViewModel _vm;

    public CareManagerPage(CareManagerViewModel vm)
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
