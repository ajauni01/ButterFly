using Butterfly.Data;
using Butterfly.Data.Entities;
using Butterfly.Shared.Dtos;
using Butterfly.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Butterfly.Api.Services;

/// <summary>
/// Values-based matching. No ML: a mentee's score is the number of tags that overlap between the
/// mentor's survey (values ∪ interests) and the profile's tags, case-insensitive. A profile in the
/// mentor's preferred talent category breaks ties. SAFEGUARDING: only Approved profiles are ever
/// considered — enforced in <see cref="GetMatchesForMentorAsync"/>.
/// </summary>
public interface IMatchingService
{
    /// <summary>
    /// Pure scoring over a candidate set. Callers must pass only Approved profiles; the ordering is
    /// deterministic: score desc, then preferred-category match, then newest, then Id for stability.
    /// </summary>
    IReadOnlyList<MenteeMatchDto> RankMatches(SurveyResponse survey, IEnumerable<MenteeProfile> approvedCandidates);

    /// <summary>Loads the mentor's survey + Approved profiles from the DB and returns ranked matches.</summary>
    Task<IReadOnlyList<MenteeMatchDto>> GetMatchesForMentorAsync(Guid mentorId, CancellationToken ct = default);
}

public sealed class MatchingService : IMatchingService
{
    private readonly ButterflyDbContext _db;

    public MatchingService(ButterflyDbContext db) => _db = db;

    public IReadOnlyList<MenteeMatchDto> RankMatches(SurveyResponse survey, IEnumerable<MenteeProfile> approvedCandidates)
    {
        // Mentor's interest surface = values ∪ interests, normalized for case-insensitive overlap.
        var mentorTags = survey.Values
            .Concat(survey.Interests)
            .Select(Normalize)
            .Where(t => t.Length > 0)
            .ToHashSet();

        var ranked = new List<(MenteeMatchDto Dto, bool PreferredHit, DateTimeOffset CreatedAt)>();

        foreach (var profile in approvedCandidates)
        {
            // Guard: never rank a non-approved profile even if a caller passes one in.
            if (profile.Status != ProfileStatus.Approved)
                continue;

            var matched = profile.Tags
                .Where(t => mentorTags.Contains(Normalize(t)))
                .ToList();

            var preferredHit = survey.PreferredTalentCategory.HasValue
                && profile.TalentCategory == survey.PreferredTalentCategory.Value;

            var dto = new MenteeMatchDto
            {
                Profile = profile.ToDto(),
                MatchScore = matched.Count,
                MatchedTags = matched
            };
            ranked.Add((dto, preferredHit, profile.CreatedAt));
        }

        return ranked
            .OrderByDescending(x => x.Dto.MatchScore)
            .ThenByDescending(x => x.PreferredHit)
            .ThenByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Dto.Profile.Id)
            .Select(x => x.Dto)
            .ToList();
    }

    public async Task<IReadOnlyList<MenteeMatchDto>> GetMatchesForMentorAsync(Guid mentorId, CancellationToken ct = default)
    {
        var survey = await _db.SurveyResponses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.MentorId == mentorId, ct);

        if (survey is null)
            return Array.Empty<MenteeMatchDto>();

        var approved = await _db.MenteeProfiles
            .AsNoTracking()
            .Where(p => p.Status == ProfileStatus.Approved)
            .ToListAsync(ct);

        return RankMatches(survey, approved);
    }

    private static string Normalize(string tag) => tag.Trim().ToLowerInvariant();
}
