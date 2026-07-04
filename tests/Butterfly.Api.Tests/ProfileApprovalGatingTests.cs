using Butterfly.Api.Controllers;
using Butterfly.Api.Infrastructure;
using Butterfly.Api.Services;
using Butterfly.Api.Tests.TestSupport;
using Butterfly.Shared.Dtos;
using Butterfly.Shared.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Butterfly.Api.Tests;

/// <summary>
/// SAFEGUARDING: a Pending/Rejected mentee profile must NEVER be exposed to a Mentor — neither via
/// the match feed nor via direct profile lookup.
/// </summary>
public class ProfileApprovalGatingTests
{
    [Fact]
    public async Task Matches_never_include_pending_profiles()
    {
        using var db = TestDb.Create();
        var mentor = Make.Mentor();
        db.Mentors.Add(mentor);
        db.SurveyResponses.Add(Make.Survey(new[] { "music" }, mentorId: mentor.Id));
        db.MenteeProfiles.Add(Make.Profile("VisibleApproved", new[] { "music" }, ProfileStatus.Approved));
        db.MenteeProfiles.Add(Make.Profile("HiddenPending", new[] { "music" }, ProfileStatus.Pending));
        db.MenteeProfiles.Add(Make.Profile("HiddenRejected", new[] { "music" }, ProfileStatus.Rejected));
        await db.SaveChangesAsync();

        var service = new MatchingService(db);
        var matches = await service.GetMatchesForMentorAsync(mentor.Id);

        matches.Should().ContainSingle();
        matches.Single().Profile.DisplayName.Should().Be("VisibleApproved");
    }

    [Fact]
    public async Task Mentor_gets_404_not_403_on_a_pending_profile()
    {
        using var db = TestDb.Create();
        var pending = Make.Profile("Pending", new[] { "music" }, ProfileStatus.Pending);
        db.MenteeProfiles.Add(pending);
        await db.SaveChangesAsync();

        var controller = new ProfilesController(
            db,
            new FakeCurrentUser { Role = UserRole.Mentor },
            new StubUserProvisioning());

        // 404 (not 403) so a pending profile's existence isn't disclosed to a mentor.
        var act = () => controller.GetProfile(pending.Id, default);
        var ex = await act.Should().ThrowAsync<ApiException>();
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Admin_can_see_a_pending_profile()
    {
        using var db = TestDb.Create();
        var pending = Make.Profile("Pending", new[] { "music" }, ProfileStatus.Pending);
        db.MenteeProfiles.Add(pending);
        await db.SaveChangesAsync();

        var controller = new ProfilesController(
            db,
            new FakeCurrentUser { Role = UserRole.Admin },
            new StubUserProvisioning());

        var result = await controller.GetProfile(pending.Id, default);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<MenteeProfileDto>()
            .Which.ProfileStatus.Should().Be(ProfileStatus.Pending);
    }
}
