using Butterfly.Shared.Dtos;

namespace Butterfly.Mobile.Services;

/// <summary>
/// App-facing gateway to the Butterfly API. Wraps the typed Refit client (<see cref="IButterflyApi"/>)
/// so view models depend on this stable abstraction rather than Refit directly.
/// </summary>
public interface IApiClient
{
    // ---- Current user ----
    Task<UserProfileDto> GetMeAsync(CancellationToken ct = default);

    // ---- Mentor ----
    Task<SurveyResponseDto> SubmitSurveyAsync(SurveyRequestDto request, CancellationToken ct = default);
    Task<IReadOnlyList<MenteeMatchDto>> GetMatchesAsync(CancellationToken ct = default);
    Task<MentorshipDto> CreateMentorshipAsync(CreateMentorshipRequestDto request, CancellationToken ct = default);
    Task<IReadOnlyList<MentorshipDto>> GetMyMentorshipsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ImpactUpdateDto>> GetImpactAsync(Guid id, CancellationToken ct = default);

    // ---- Profiles ----
    Task<MenteeProfileDto> GetProfileAsync(Guid id, CancellationToken ct = default);

    // ---- Care manager ----
    Task<MenteeProfileDto> CreateProfileAsync(CreateMenteeProfileRequestDto request, CancellationToken ct = default);
    Task<IReadOnlyList<MenteeProfileDto>> GetMyProfilesAsync(CancellationToken ct = default);
    Task<ImpactUpdateDto> LogImpactAsync(Guid id, CreateImpactUpdateRequestDto request, CancellationToken ct = default);

    // ---- Admin ----
    Task<IReadOnlyList<MenteeProfileDto>> GetPendingProfilesAsync(CancellationToken ct = default);
    Task<MenteeProfileDto> ApproveProfileAsync(Guid id, CancellationToken ct = default);
    Task<MenteeProfileDto> RejectProfileAsync(Guid id, RejectProfileRequestDto request, CancellationToken ct = default);
    Task<CareManagerDto> VerifyCareManagerAsync(Guid id, CancellationToken ct = default);
    Task<PaymentDto> RecordPaymentAsync(CreatePaymentRequestDto request, CancellationToken ct = default);
}
