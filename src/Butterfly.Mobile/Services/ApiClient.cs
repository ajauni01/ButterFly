using Butterfly.Shared.Dtos;

namespace Butterfly.Mobile.Services;

/// <summary>
/// Thin delegating implementation of <see cref="IApiClient"/> over the typed Refit client. The
/// bearer token is attached by <see cref="AuthHeaderHandler"/> on the underlying HTTP pipeline, so
/// nothing here deals with authorization headers.
/// </summary>
public sealed class ApiClient : IApiClient
{
    private readonly IButterflyApi _api;

    public ApiClient(IButterflyApi api) => _api = api;

    // ---- Current user ----
    public Task<UserProfileDto> GetMeAsync(CancellationToken ct = default) => _api.GetMeAsync(ct);

    // ---- Mentor ----
    public Task<SurveyResponseDto> SubmitSurveyAsync(SurveyRequestDto request, CancellationToken ct = default) =>
        _api.SubmitSurveyAsync(request, ct);

    public Task<IReadOnlyList<MenteeMatchDto>> GetMatchesAsync(CancellationToken ct = default) =>
        _api.GetMatchesAsync(ct);

    public Task<MentorshipDto> CreateMentorshipAsync(CreateMentorshipRequestDto request, CancellationToken ct = default) =>
        _api.CreateMentorshipAsync(request, ct);

    public Task<IReadOnlyList<MentorshipDto>> GetMyMentorshipsAsync(CancellationToken ct = default) =>
        _api.GetMyMentorshipsAsync(ct);

    public Task<IReadOnlyList<ImpactUpdateDto>> GetImpactAsync(Guid id, CancellationToken ct = default) =>
        _api.GetImpactAsync(id, ct);

    // ---- Profiles ----
    public Task<MenteeProfileDto> GetProfileAsync(Guid id, CancellationToken ct = default) =>
        _api.GetProfileAsync(id, ct);

    // ---- Care manager ----
    public Task<MenteeProfileDto> CreateProfileAsync(CreateMenteeProfileRequestDto request, CancellationToken ct = default) =>
        _api.CreateProfileAsync(request, ct);

    public Task<IReadOnlyList<MenteeProfileDto>> GetMyProfilesAsync(CancellationToken ct = default) =>
        _api.GetMyProfilesAsync(ct);

    public Task<ImpactUpdateDto> LogImpactAsync(Guid id, CreateImpactUpdateRequestDto request, CancellationToken ct = default) =>
        _api.LogImpactAsync(id, request, ct);

    // ---- Admin ----
    public Task<IReadOnlyList<MenteeProfileDto>> GetPendingProfilesAsync(CancellationToken ct = default) =>
        _api.GetPendingProfilesAsync(ct);

    public Task<MenteeProfileDto> ApproveProfileAsync(Guid id, CancellationToken ct = default) =>
        _api.ApproveProfileAsync(id, ct);

    public Task<MenteeProfileDto> RejectProfileAsync(Guid id, RejectProfileRequestDto request, CancellationToken ct = default) =>
        _api.RejectProfileAsync(id, request, ct);

    public Task<CareManagerDto> VerifyCareManagerAsync(Guid id, CancellationToken ct = default) =>
        _api.VerifyCareManagerAsync(id, ct);

    public Task<PaymentDto> RecordPaymentAsync(CreatePaymentRequestDto request, CancellationToken ct = default) =>
        _api.RecordPaymentAsync(request, ct);
}
