using Butterfly.Mobile.ViewModels;

namespace Butterfly.Mobile.Views;

public partial class MentorPage : ContentPage
{
    private readonly MentorViewModel _vm;
    private readonly IServiceProvider _services;

    public MentorPage(MentorViewModel vm, IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _services = services;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }

    private async void OnTakeSurvey(object? sender, EventArgs e)
    {
        var page = _services.GetRequiredService<SurveyPage>();
        await Navigation.PushModalAsync(new NavigationPage(page));
        // Refresh after the survey is dismissed.
        page.Disappearing += async (_, _) => await _vm.LoadCommand.ExecuteAsync(null);
    }
}
