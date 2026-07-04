using System.Collections.ObjectModel;
using Butterfly.Mobile.Services;
using Butterfly.Shared.Dtos;
using CommunityToolkit.Mvvm.Input;

namespace Butterfly.Mobile.ViewModels;

/// <summary>Admin dashboard: the pending-profile approval queue with approve/reject actions.</summary>
public sealed partial class AdminViewModel : BaseViewModel
{
    private readonly IButterflyApi _api;

    public AdminViewModel(IButterflyApi api)
    {
        _api = api;
        Title = "Pending Approvals";
    }

    public ObservableCollection<MenteeProfileDto> Pending { get; } = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            Pending.Clear();
            foreach (var p in await _api.GetPendingProfilesAsync())
                Pending.Add(p);
            IsEmpty = Pending.Count == 0;
        });
    }

    [RelayCommand]
    private async Task ApproveAsync(MenteeProfileDto? profile)
    {
        if (profile is null) return;
        var ok = await RunAsync(async () => await _api.ApproveProfileAsync(profile.Id));
        if (ok) Pending.Remove(profile);
    }

    [RelayCommand]
    private async Task RejectAsync(MenteeProfileDto? profile)
    {
        if (profile is null) return;

        var page = Application.Current?.MainPage;
        var reason = page is null
            ? null
            : await page.DisplayPromptAsync("Reject profile", "Reason for rejection:", "Reject", "Cancel");
        if (string.IsNullOrWhiteSpace(reason)) return;

        var ok = await RunAsync(async () =>
            await _api.RejectProfileAsync(profile.Id, new RejectProfileRequestDto { Reason = reason }));
        if (ok) Pending.Remove(profile);
    }
}
