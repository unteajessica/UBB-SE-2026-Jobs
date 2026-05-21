using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Repositories.Matches;
using PussyCats.Library.Services.JobSkills;
using PussyCats.Library.Services.UserSkillService;

namespace PussyCats.Library.Services.SkillGapService;

public class SkillGapService : ISkillGapService
{
    private readonly IMatchRepository matchRepository;
    private readonly IJobSkillService jobSkillService;
    private readonly IUserSkillService userSkillService;

    public SkillGapService(
        IMatchRepository matchRepository,
        IJobSkillService jobSkillService,
        IUserSkillService userSkillService)
    {
        this.matchRepository = matchRepository;
        this.jobSkillService = jobSkillService;
        this.userSkillService = userSkillService;
    }

    public async Task<IReadOnlyList<MissingSkillModel>> GetMissingSkillsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var rejectedMatches = await GetRejectedMatchesAsync(userId, cancellationToken).ConfigureAwait(false);
        if (rejectedMatches.Count == 0)
        {
            return new List<MissingSkillModel>();
        }

        var userSkillIds = new HashSet<int>();
        foreach (var userSkill in await userSkillService.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false))
        {
            userSkillIds.Add(userSkill.Skill.SkillId);
        }

        var missingCount = new Dictionary<string, int>();
        foreach (var match in rejectedMatches)
        {
            foreach (var jobSkill in await jobSkillService.GetByJobIdAsync(match.Job.JobId, cancellationToken).ConfigureAwait(false))
            {
                if (!userSkillIds.Contains(jobSkill.Skill.SkillId))
                {
                    var skillName = jobSkill.Skill.Name;
                    if (!missingCount.ContainsKey(skillName))
                    {
                        missingCount[skillName] = 0;
                    }

                    missingCount[skillName]++;
                }
            }
        }

        var missingSkills = new List<MissingSkillModel>();
        foreach (var missing in missingCount)
        {
            missingSkills.Add(new MissingSkillModel { SkillName = missing.Key, RejectedJobCount = missing.Value });
        }

        missingSkills.Sort(CompareMissingSkillCountDescending);
        return missingSkills;
    }

    public async Task<IReadOnlyList<UnderscoredSkillModel>> GetUnderscoredSkillsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var rejectedMatches = await GetRejectedMatchesAsync(userId, cancellationToken).ConfigureAwait(false);
        if (rejectedMatches.Count == 0)
        {
            return new List<UnderscoredSkillModel>();
        }

        var userSkillMap = new Dictionary<int, UserSkill>();
        foreach (var userSkill in await userSkillService.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false))
        {
            userSkillMap[userSkill.Skill.SkillId] = userSkill;
        }

        var requiredScoresPerSkill = new Dictionary<int, (string Name, int UserScore, List<int> RequiredScores)>();
        foreach (var match in rejectedMatches)
        {
            foreach (var jobSkill in await jobSkillService.GetByJobIdAsync(match.Job.JobId, cancellationToken).ConfigureAwait(false))
            {
                if (!userSkillMap.TryGetValue(jobSkill.Skill.SkillId, out var userSkill))
                {
                    continue;
                }

                if (userSkill.Score >= jobSkill.RequiredLevel)
                {
                    continue;
                }

                if (!requiredScoresPerSkill.ContainsKey(jobSkill.Skill.SkillId))
                {
                    requiredScoresPerSkill[jobSkill.Skill.SkillId] = (jobSkill.Skill.Name, userSkill.Score, new List<int>());
                }

                requiredScoresPerSkill[jobSkill.Skill.SkillId].RequiredScores.Add(jobSkill.RequiredLevel);
            }
        }

        var underscoredSkills = new List<UnderscoredSkillModel>();
        foreach (var skill in requiredScoresPerSkill)
        {
            underscoredSkills.Add(new UnderscoredSkillModel
            {
                SkillName = skill.Value.Name,
                UserScore = skill.Value.UserScore,
                AverageRequiredScore = ComputeAverage(skill.Value.RequiredScores),
            });
        }

        underscoredSkills.Sort(CompareSkillGapDescending);
        return underscoredSkills;
    }

    public async Task<SkillGapSummaryModel> GetSummaryAsync(int userId, CancellationToken cancellationToken = default)
    {
        var rejectedMatches = await GetRejectedMatchesAsync(userId, cancellationToken).ConfigureAwait(false);
        if (rejectedMatches.Count == 0)
        {
            return new SkillGapSummaryModel { HasRejections = false, HasSkillGaps = false };
        }

        var missing = await GetMissingSkillsAsync(userId, cancellationToken).ConfigureAwait(false);
        var underscored = await GetUnderscoredSkillsAsync(userId, cancellationToken).ConfigureAwait(false);

        return new SkillGapSummaryModel
        {
            HasRejections = true,
            HasSkillGaps = missing.Count > 0 || underscored.Count > 0,
            MissingSkillsCount = missing.Count,
            SkillsToImproveCount = underscored.Count,
        };
    }

    private async Task<IReadOnlyList<Match>> GetRejectedMatchesAsync(int userId, CancellationToken cancellationToken)
    {
        var matches = await matchRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var rejected = new List<Match>();
        foreach (var match in matches)
        {
            if (match.Status == MatchStatus.Rejected)
            {
                rejected.Add(match);
            }
        }

        return rejected;
    }

    private static int CompareMissingSkillCountDescending(MissingSkillModel left, MissingSkillModel right)
    {
        return right.RejectedJobCount.CompareTo(left.RejectedJobCount);
    }

    private static int CompareSkillGapDescending(UnderscoredSkillModel left, UnderscoredSkillModel right)
    {
        var leftGap = left.AverageRequiredScore - left.UserScore;
        var rightGap = right.AverageRequiredScore - right.UserScore;
        return rightGap.CompareTo(leftGap);
    }

    private static int ComputeAverage(IReadOnlyList<int> values)
    {
        var sum = 0;
        foreach (var value in values)
        {
            sum += value;
        }

        return sum / values.Count;
    }
}
