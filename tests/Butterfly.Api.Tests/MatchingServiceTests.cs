using Butterfly.Api.Services;
using Butterfly.Api.Tests.TestSupport;
using Butterfly.Shared.Enums;
using FluentAssertions;
using Xunit;

namespace Butterfly.Api.Tests;

public class MatchingServiceTests
{
    private static MatchingService NewService() => new(TestDb.Create());

    [Fact]
    public void RankMatches_orders_by_descending_tag_overlap()
    {
        var survey = Make.Survey(
            values: new[] { "music", "arts", "discipline" },
            interests: new[] { "education" });

        var strong = Make.Profile("Strong", new[] { "music", "arts", "education" }); // 3
        var medium = Make.Profile("Medium", new[] { "music", "discipline" });         // 2
        var weak = Make.Profile("Weak", new[] { "arts" });                            // 1
        var none = Make.Profile("None", new[] { "sports", "cooking" });               // 0

        var service = NewService();
        var result = service.RankMatches(survey, new[] { weak, none, strong, medium });

        result.Select(r => r.Profile.DisplayName)
            .Should().ContainInOrder("Strong", "Medium", "Weak", "None");
        result[0].MatchScore.Should().Be(3);
        result[0].MatchedTags.Should().BeEquivalentTo(new[] { "music", "arts", "education" });
    }

    [Fact]
    public void RankMatches_is_case_insensitive_on_tags()
    {
        var survey = Make.Survey(values: new[] { "Music", "ARTS" });
        var profile = Make.Profile("Mixed", new[] { "music", "arts" });

        var result = NewService().RankMatches(survey, new[] { profile });

        result.Single().MatchScore.Should().Be(2);
    }

    [Fact]
    public void RankMatches_breaks_ties_using_preferred_talent_category()
    {
        var survey = Make.Survey(
            values: new[] { "music" },
            preferred: TalentCategory.Musician);

        // Same score (1) but only one is in the preferred category.
        var preferred = Make.Profile("Preferred", new[] { "music" }, category: TalentCategory.Musician);
        var other = Make.Profile("Other", new[] { "music" }, category: TalentCategory.Student);

        var result = NewService().RankMatches(survey, new[] { other, preferred });

        result.First().Profile.DisplayName.Should().Be("Preferred");
    }

    [Fact]
    public void RankMatches_skips_non_approved_profiles_even_if_passed_in()
    {
        var survey = Make.Survey(values: new[] { "music" });
        var pending = Make.Profile("Pending", new[] { "music" }, status: ProfileStatus.Pending);
        var approved = Make.Profile("Approved", new[] { "music" }, status: ProfileStatus.Approved);

        var result = NewService().RankMatches(survey, new[] { pending, approved });

        result.Should().ContainSingle();
        result.Single().Profile.DisplayName.Should().Be("Approved");
    }
}
