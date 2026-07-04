using Butterfly.Mobile.Services;
using Butterfly.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Butterfly.Mobile.ViewModels;

/// <summary>Mentor values/interests survey. Comma-separated entry kept simple for the pilot.</summary>
public sealed partial class SurveyViewModel : BaseViewModel
{
    private readonly IButterflyApi _api;
    private readonly IAppNavigator _navigator;

    public SurveyViewModel(IButterflyApi api, IAppNavigator navigator)
    {
        _api = api;
        _navigator = navigator;
        Title = "Your Values & Interests";
    }

    [ObservableProperty] private string _values = string.Empty;
    [ObservableProperty] private string _interests = string.Empty;

    [RelayCommand]
    private async Task SaveAsync()
    {
        var values = Split(Values);
        var interests = Split(Interests);
        if (values.Count == 0 || interests.Count == 0)
        {
            ErrorMessage = "Please enter at least one value and one interest.";
            return;
        }

        var ok = await RunAsync(async () =>
        {
            await _api.SubmitSurveyAsync(new SurveyRequestDto
            {
                Values = values,
                Interests = interests,
                PreferredTalentCategory = null
            });
        });

        if (ok && Application.Current?.MainPage is not null)
            await Application.Current.MainPage.Navigation.PopModalAsync();
    }

    private static List<string> Split(string csv) =>
        csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
