using System.Collections.ObjectModel;
using Butterfly.Mobile.Services;
using Butterfly.Shared.Dtos;
using Butterfly.Shared.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Butterfly.Mobile.ViewModels;

/// <summary>
/// Mentor dashboard: prompts for the survey on first run, then shows tag-matched approved mentees
/// and the mentor's own mentorships.
/// </summary>
public sealed partial class MentorViewModel : BaseViewModel
{
    private readonly IButterflyApi _api;

    public MentorViewModel(IButterflyApi api)
    {
        _api = api;
        Title = "Find a Mentee";
    }

    public ObservableCollection<MenteeMatchDto> Matches { get; } = new();
    public ObservableCollection<MentorshipDto> Mentorships { get; } = new();

    [ObservableProperty] private bool _needsSurvey;

    [RelayCommand]
    public async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            var me = await _api.GetMeAsync();
            NeedsSurvey = !me.HasCompletedSurvey;

            Matches.Clear();
            if (!NeedsSurvey)
            {
                foreach (var m in await _api.GetMatchesAsync())
                    Matches.Add(m);
            }

            Mentorships.Clear();
            foreach (var ms in await _api.GetMyMentorshipsAsync())
                Mentorships.Add(ms);

            IsEmpty = Matches.Count == 0 && Mentorships.Count == 0 && !NeedsSurvey;
        });
    }

    /// <summary>Quick-start a guidance mentorship with a matched mentee.</summary>
    [RelayCommand]
    private async Task MentorAsync(MenteeMatchDto? match)
    {
        if (match is null) return;
        var ok = await RunAsync(async () =>
        {
            await _api.CreateMentorshipAsync(new CreateMentorshipRequestDto
            {
                MenteeProfileId = match.Profile.Id,
                RelationshipType = RelationshipType.Guidance,
                MeetingCadence = MeetingCadence.Monthly,
                MonthlyAmountUSD = null
            });
        });
        if (ok) await LoadAsync();
    }
}
