using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Repositories.Skills;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.Library.Services.CompatibilityService;

public class CompatibilityService : ICompatibilityService
{
    private const int SkillsLineIndex = 2;
    private const char SkillDelimiter = ',';
    private const double UnverifiedSkillScore = 0.5;
    private const double ScoreNormalizationFactor = 100.0;
    private const double HighSkillCoverageThreshold = 0.5;
    private const double TargetGroupScore = 0.8;
    private const int MaxSuggestions = 3;
    private const int InvalidScore = -1;

    private readonly IUserSkillRepository userSkillRepository;
    private readonly ISkillGroupRepository skillGroupRepository;
    private readonly IUserRepository userRepository;

    public CompatibilityService(
        IUserSkillRepository userSkillRepository,
        ISkillGroupRepository skillGroupRepository,
        IUserRepository userRepository)
    {
        this.userSkillRepository = userSkillRepository;
        this.skillGroupRepository = skillGroupRepository;
        this.userRepository = userRepository;
    }

    public async Task<RoleResult> CalculateForRoleAsync(int userId, JobRole role, CancellationToken cancellationToken = default)
    {
        var userSkills = await GetUserSkillsAsync(userId, cancellationToken).ConfigureAwait(false);
        var groups = await skillGroupRepository.GetByJobRoleAsync(role, cancellationToken).ConfigureAwait(false);

        int totalWeight = 0;
        foreach (var group in groups)
            totalWeight += group.Weight;

        var groupScores = new List<double>();
        foreach (var group in groups)
            groupScores.Add(ComputeGroupScore(group, userSkills));

        double matchScore = ComputeMatchScore(groups, groupScores);

        var result = new RoleResult { JobRole = role };

        if (matchScore == InvalidScore)
        {
            result.MatchScore = InvalidScore;
            result.Suggestions = new List<Suggestion>();
            return result;
        }

        result.MatchScore = matchScore;
        result.Suggestions = IdentifyGaps(groups, userSkills, totalWeight);
        return result;
    }

    public async Task<IReadOnlyList<RoleResult>> CalculateAllAsync(int userId, CancellationToken cancellationToken = default)
    {
        var results = new List<RoleResult>();
        foreach (JobRole role in Enum.GetValues(typeof(JobRole)))
            results.Add(await CalculateForRoleAsync(userId, role, cancellationToken).ConfigureAwait(false));
        return results;
    }

    public IReadOnlyList<Suggestion> GetSuggestions(RoleResult result) => result.Suggestions;

    private async Task<List<UserSkill>> GetUserSkillsAsync(int userId, CancellationToken cancellationToken)
    {
        var verifiedSkills = await userSkillRepository.GetVerifiedByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var user = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var cvSkills = ExtractSkillsFromParsedCv(user?.ParsedCv ?? string.Empty);
        return MergeVerifiedAndUnverifiedSkills(verifiedSkills, cvSkills);
    }

    private static List<string> ExtractSkillsFromParsedCv(string parsedCv)
    {
        if (string.IsNullOrWhiteSpace(parsedCv))
            return new List<string>();

        string[] lines = parsedCv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        if (lines.Length <= SkillsLineIndex)
            return new List<string>();

        string skillsLine = lines[SkillsLineIndex].Trim();
        if (string.IsNullOrWhiteSpace(skillsLine))
            return new List<string>();

        return skillsLine.Split(SkillDelimiter)
            .Select(skillToAdjust => skillToAdjust.Trim())
            .Where(skillToAdjust => !string.IsNullOrWhiteSpace(skillToAdjust))
            .ToList();
    }

    private static List<UserSkill> MergeVerifiedAndUnverifiedSkills(IReadOnlyList<UserSkill> verifiedSkills, List<string> cvSkills)
    {
        var allSkills = verifiedSkills.ToList();
        foreach (string cvSkill in cvSkills)
        {
            bool alreadyPresent = allSkills.Any(userSkillWithPossibleSkill => string.Equals(userSkillWithPossibleSkill.Skill?.Name, cvSkill, StringComparison.OrdinalIgnoreCase));
            if (!alreadyPresent)
                allSkills.Add(new UserSkill { Skill = new Skill { Name = cvSkill }, IsVerified = false, Score = 0 });
        }
        return allSkills;
    }

    private static double ComputeGroupScore(SkillGroup group, List<UserSkill> userSkills)
    {
        double max = 0;
        foreach (var skill in group.Skills)
        {
            var match = userSkills.FirstOrDefault(userSkill => string.Equals(userSkill.Skill?.Name, skill.Name, StringComparison.OrdinalIgnoreCase));
            if (match is null) continue;
            double score = match.IsVerified ? match.Score / ScoreNormalizationFactor : UnverifiedSkillScore;
            if (score > max) max = score;
        }
        return max;
    }

    private static double ComputeMatchScore(IReadOnlyList<SkillGroup> groups, List<double> groupScores)
    {
        int totalWeight = groups.Sum(groupWithWeight => groupWithWeight.Weight);
        if (totalWeight == 0) return InvalidScore;
        double weighted = groups.Select((groupWithWeight, indexOfGroup) => groupWithWeight.Weight * groupScores[indexOfGroup]).Sum();
        return weighted * ScoreNormalizationFactor / totalWeight;
    }

    private static List<Suggestion> IdentifyGaps(IReadOnlyList<SkillGroup> skillGroups, List<UserSkill> userSkills, int totalWeight)
    {
        var suggestions = new List<Suggestion>();
        foreach (var group in skillGroups)
        {
            double groupScore = ComputeGroupScore(group, userSkills);
            if (groupScore > HighSkillCoverageThreshold) continue;

            var skill = group.Skills.FirstOrDefault(skillToAdjust => !userSkills.Any(userSkillWithPossibleSkill => string.Equals(userSkillWithPossibleSkill.Skill?.Name, skillToAdjust.Name, StringComparison.OrdinalIgnoreCase)));
            if (skill is null) continue;

            suggestions.Add(new Suggestion
            {
                SkillName = skill.Name,
                GroupName = group.GroupName,
                GainScore = ScoreNormalizationFactor * group.Weight * (TargetGroupScore - groupScore) / totalWeight,
            });
        }

        return suggestions.OrderByDescending(suggestionsCheckScore => suggestionsCheckScore.GainScore).Take(MaxSuggestions).ToList();
    }
}
