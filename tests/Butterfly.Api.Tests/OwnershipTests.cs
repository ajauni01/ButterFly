using Butterfly.Api.Controllers;
using Butterfly.Api.Infrastructure;
using Butterfly.Api.Services;
using Butterfly.Api.Tests.TestSupport;
using Butterfly.Shared.Dtos;
using Butterfly.Shared.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Butterfly.Api.Tests;

/// <summary>
/// Ownership: a mentor may only read their own mentorship's impact feed; a care manager may only
/// log impact for a mentee they created.
/// </summary>
public class OwnershipTests
{
    [Fact]
    public async Task Mentor_cannot_read_another_mentors_impact_feed()
    {
        using var db = TestDb.Create();
        var owner = Make.Mentor();
        var intruder = Make.Mentor();
        var profile = Make.Profile("Mentee", new[] { "music" });
        var mentorship = Make.Mentorship(owner.Id, profile);
        db.Mentors.AddRange(owner, intruder);
        db.MenteeProfiles.Add(profile);
        db.Mentorships.Add(mentorship);
        await db.SaveChangesAsync();

        var controller = new MentorsController(
            db,
            new StubUserProvisioning { CurrentMentor = intruder },
            new MatchingService(db));

        var act = () => controller.GetImpact(mentorship.Id, default);
        var ex = await act.Should().ThrowAsync<ApiException>();
        ex.Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Mentor_can_read_their_own_impact_feed()
    {
        using var db = TestDb.Create();
        var owner = Make.Mentor();
        var profile = Make.Profile("Mentee", new[] { "music" });
        var mentorship = Make.Mentorship(owner.Id, profile);
        db.Mentors.Add(owner);
        db.MenteeProfiles.Add(profile);
        db.Mentorships.Add(mentorship);
        await db.SaveChangesAsync();

        var controller = new MentorsController(
            db,
            new StubUserProvisioning { CurrentMentor = owner },
            new MatchingService(db));

        var result = await controller.GetImpact(mentorship.Id, default);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CareManager_cannot_log_impact_for_a_mentee_they_do_not_manage()
    {
        using var db = TestDb.Create();
        var owningCm = Make.CareManager();
        var intruderCm = Make.CareManager();
        var profile = Make.Profile("Mentee", new[] { "music" }, careManagerId: owningCm.Id);
        var mentorship = Make.Mentorship(Make.Mentor().Id, profile);
        db.CareManagers.AddRange(owningCm, intruderCm);
        db.MenteeProfiles.Add(profile);
        db.Mentorships.Add(mentorship);
        await db.SaveChangesAsync();

        var controller = new CareManagersController(
            db,
            new StubUserProvisioning { CurrentCareManager = intruderCm });

        var request = new CreateImpactUpdateRequestDto
        {
            WeekOf = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdateType = ImpactUpdateType.Progress,
            ImpactNote = "Trying to post to someone else's mentee."
        };

        var act = () => controller.LogImpact(mentorship.Id, request, default);
        var ex = await act.Should().ThrowAsync<ApiException>();
        ex.Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task CareManager_can_log_impact_for_their_own_mentee()
    {
        using var db = TestDb.Create();
        var cm = Make.CareManager();
        var profile = Make.Profile("Mentee", new[] { "music" }, careManagerId: cm.Id);
        var mentorship = Make.Mentorship(Make.Mentor().Id, profile);
        db.CareManagers.Add(cm);
        db.MenteeProfiles.Add(profile);
        db.Mentorships.Add(mentorship);
        await db.SaveChangesAsync();

        var controller = new CareManagersController(
            db,
            new StubUserProvisioning { CurrentCareManager = cm });

        var request = new CreateImpactUpdateRequestDto
        {
            WeekOf = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdateType = ImpactUpdateType.Progress,
            ImpactNote = "Great progress this week."
        };

        var result = await controller.LogImpact(mentorship.Id, request, default);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
        (await db.ImpactUpdates.CountAsync()).Should().Be(1);
    }
}
