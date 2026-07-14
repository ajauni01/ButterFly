using System.Collections.ObjectModel;
using Butterfly.Mobile.Services;
using Butterfly.Shared.Dtos;
using Butterfly.Shared.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Butterfly.Mobile.ViewModels;

/// <summary>Care manager dashboard: list own mentee profiles (with status badges) and create new ones.</summary>
public sealed partial class CareManagerViewModel : BaseViewModel
{
    private readonly IApiClient _api;

    public CareManagerViewModel(IApiClient api)
    {
        _api = api;
        Title = "My Mentees";
    }

    public ObservableCollection<MenteeProfileDto> Profiles { get; } = new();

    // ---- New-profile form fields ----
    [ObservableProperty] private string _displayName = string.Empty;
    [ObservableProperty] private string _age = string.Empty;
    [ObservableProperty] private string _region = string.Empty;
    [ObservableProperty] private string _story = string.Empty;
    [ObservableProperty] private string _tags = string.Empty;
    [ObservableProperty] private bool _isFormVisible;

    [RelayCommand]
    public async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            Profiles.Clear();
            foreach (var p in await _api.GetMyProfilesAsync())
                Profiles.Add(p);
            IsEmpty = Profiles.Count == 0;
        });
    }

    [RelayCommand]
    private void ToggleForm() => IsFormVisible = !IsFormVisible;

    [RelayCommand]
    private async Task CreateAsync()
    {
        if (!int.TryParse(Age, out var age) || age < 1 || age > 17)
        {
            ErrorMessage = "Age must be a number between 1 and 17 (mentees are minors).";
            return;
        }
        var tags = Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (tags.Count == 0)
        {
            ErrorMessage = "Please add at least one tag.";
            return;
        }

        var ok = await RunAsync(async () =>
        {
            await _api.CreateProfileAsync(new CreateMenteeProfileRequestDto
            {
                DisplayName = DisplayName,
                Age = age,
                Region = Region,
                TalentCategory = TalentCategory.Other,
                Story = Story,
                Tags = tags,
                SupportNeeded = SupportNeeded.Mentorship, // guidance-only default; no money field
                MonthlyNeedBDT = null
            });
        });

        if (ok)
        {
            DisplayName = Age = Region = Story = Tags = string.Empty;
            IsFormVisible = false;
            await LoadAsync();
        }
    }
}
