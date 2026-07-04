using Butterfly.Shared.Dtos;
using Refit;

namespace Butterfly.Mobile.Services;

/// <summary>
/// Typed Refit client mirroring the API. The bearer token is attached by
/// <see cref="AuthHeaderHandler"/>, so no method here takes an Authorization argument.
/// </summary>
public interface IButterflyApi
{
    // ---- Current user ----
    [Get("/api/me")]
    Task<UserProfileDto> GetMeAsync(CancellationToken ct = default);

    // ---- Mentor ----
    [Post("/api/mentors/survey")]
    Task<SurveyResponseDto> SubmitSurveyAsync([Body] SurveyRequestDto request, CancellationToken ct = default);

    [Get("/api/mentors/matches")]
    Task<IReadOnlyList<MenteeMatchDto>> GetMatchesAsync(CancellationToken ct = default);

    [Post("/api/mentorships")]
    Task<MentorshipDto> CreateMentorshipAsync([Body] CreateMentorshipRequestDto request, CancellationToken ct = default);

    [Get("/api/mentorships/mine")]
    Task<IReadOnlyList<MentorshipDto>> GetMyMentorshipsAsync(CancellationToken ct = default);

    [Get("/api/mentorships/{id}/impact")]
    Task<IReadOnlyList<ImpactUpdateDto>> GetImpactAsync(Guid id, CancellationToken ct = default);

    // ---- Profiles ----
    [Get("/api/profiles/{id}")]
    Task<MenteeProfileDto> GetProfileAsync(Guid id, CancellationToken ct = default);

    // ---- Care manager ----
    [Post("/api/caremanagers/profiles")]
    Task<MenteeProfileDto> CreateProfileAsync([Body] CreateMenteeProfileRequestDto request, CancellationToken ct = default);

    [Get("/api/caremanagers/profiles/mine")]
    Task<IReadOnlyList<MenteeProfileDto>> GetMyProfilesAsync(CancellationToken ct = default);

    [Post("/api/mentorships/{id}/impact")]
    Task<ImpactUpdateDto> LogImpactAsync(Guid id, [Body] CreateImpactUpdateRequestDto request, CancellationToken ct = default);

    // ---- Admin ----
    [Get("/api/admin/profiles/pending")]
    Task<IReadOnlyList<MenteeProfileDto>> GetPendingProfilesAsync(CancellationToken ct = default);

    [Post("/api/admin/profiles/{id}/approve")]
    Task<MenteeProfileDto> ApproveProfileAsync(Guid id, CancellationToken ct = default);

    [Post("/api/admin/profiles/{id}/reject")]
    Task<MenteeProfileDto> RejectProfileAsync(Guid id, [Body] RejectProfileRequestDto request, CancellationToken ct = default);

    [Post("/api/admin/caremanagers/{id}/verify")]
    Task<CareManagerDto> VerifyCareManagerAsync(Guid id, CancellationToken ct = default);

    [Post("/api/admin/payments")]
    Task<PaymentDto> RecordPaymentAsync([Body] CreatePaymentRequestDto request, CancellationToken ct = default);
}
