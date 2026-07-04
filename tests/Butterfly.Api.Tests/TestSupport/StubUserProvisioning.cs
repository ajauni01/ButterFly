using Butterfly.Api.Infrastructure;
using Butterfly.Api.Services;
using Butterfly.Data.Entities;
using Butterfly.Shared.Dtos;

namespace Butterfly.Api.Tests.TestSupport;

/// <summary>
/// Test double for <see cref="IUserProvisioningService"/> that returns a preset "current" identity,
/// so controller tests can control exactly who the caller is for ownership checks.
/// </summary>
public sealed class StubUserProvisioning : IUserProvisioningService
{
    public AppUser? CurrentUser { get; init; }
    public Mentor? CurrentMentor { get; init; }
    public CareManager? CurrentCareManager { get; init; }

    public Task<AppUser> EnsureCurrentAppUserAsync(CancellationToken ct = default) =>
        Task.FromResult(CurrentUser ?? throw ApiException.Forbidden("No current app user."));

    public Task<UserProfileDto> GetOrProvisionCurrentUserAsync(CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<Mentor> GetCurrentMentorAsync(CancellationToken ct = default) =>
        Task.FromResult(CurrentMentor ?? throw ApiException.Forbidden("The current user is not a mentor."));

    public Task<CareManager> GetCurrentCareManagerAsync(CancellationToken ct = default) =>
        Task.FromResult(CurrentCareManager ?? throw ApiException.Forbidden("The current user is not a care manager."));
}
