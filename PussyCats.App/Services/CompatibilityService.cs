using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Repositories.Skills;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.App.Services;

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

    private async Task<List<UserSkill>> GetUserSkillsAsync(int userId, CancellationToken cancellationToken)
    {
        var verifiedSkillsOfUser = await userSkillRepository.GetVerifiedByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        var user = await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        string parsedCvText = user?.ParsedCv ?? string.Empty;
        var extractedCvSkills = ExtractSkillsFromParsedCv(parsedCvText);
        return MergeVerifiedAndUnverifiedSkills(verifiedSkillsOfUser, extractedCvSkills);
    }

    private static List<string> ExtractSkillsFromParsedCv(string parsedCv)
    {
        var extractedSkills = new List<string>();

        if (string.IsNullOrWhiteSpace(parsedCv))
        {
            return extractedSkills;
        }

        string[] lines = parsedCv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        if (lines.Length <= SkillsLineIndex)
        {
            return extractedSkills;
        }

        string skillsLine = lines[SkillsLineIndex].Trim();

        if (string.IsNullOrWhiteSpace(skillsLine))
        {
            return extractedSkills;
        }

        string[] cvSkills = skillsLine.Split(SkillDelimiter);

        foreach (string cvSkill in cvSkills)
        {
            string skillName = cvSkill.Trim();
            if (!string.IsNullOrWhiteSpace(skillName))
            {
                extractedSkills.Add(skillName);
            }
        }

        return extractedSkills;
    }

    private static List<UserSkill> MergeVerifiedAndUnverifiedSkills(IReadOnlyList<UserSkill> verifiedSkills, List<string> cvSkills)
    {
        var allSkills = new List<UserSkill>();

        foreach (var verifiedSkill in verifiedSkills)
        {
            allSkills.Add(verifiedSkill);
        }

        foreach (string cvSkill in cvSkills)
        {
            bool isSkillAlreadyInAllSkills = false;

            foreach (var existingSkill in allSkills)
            {
                if (string.Equals(existingSkill.Skill?.Name, cvSkill, StringComparison.OrdinalIgnoreCase))
                {
                    isSkillAlreadyInAllSkills = true;
                    break;
                }
            }

            if (!isSkillAlreadyInAllSkills)
            {
                allSkills.Add(new UserSkill
                {
                    Skill = new Skill { Name = cvSkill },
                    IsVerified = false,
                    Score = 0,
                });
            }
        }

        return allSkills;
    }

    private static double ComputeGroupScore(SkillGroup group, List<UserSkill> userSkills)
    {
        double maximumSkillScore = 0;

        foreach (var skill in group.Skills)
        {
            double skillScore = 0;

            foreach (var userSkill in userSkills)
            {
                if (string.Equals(userSkill.Skill?.Name, skill.Name, StringComparison.OrdinalIgnoreCase))
                {
                    skillScore = userSkill.IsVerified
                        ? userSkill.Score / ScoreNormalizationFactor
                        : UnverifiedSkillScore;
                    break;
                }
            }

            if (skillScore > maximumSkillScore)
            {
                maximumSkillScore = skillScore;
            }
        }

        return maximumSkillScore;
    }

    private static double ComputeMatchScore(IReadOnlyList<SkillGroup> groups, List<double> groupScores)
    {
        int totalWeight = 0;
        foreach (var group in groups)
        {
            totalWeight += group.Weight;
        }

        if (totalWeight == 0)
        {
            return InvalidScore;
        }

        double weightedSum = 0;
        for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            weightedSum += groups[groupIndex].Weight * groupScores[groupIndex];
        }

        return weightedSum * ScoreNormalizationFactor / totalWeight;
    }

    private static double ComputeGain(SkillGroup group, double groupScore, int totalWeight)
    {
        return ScoreNormalizationFactor * group.Weight * (TargetGroupScore - groupScore) / totalWeight;
    }

    private static int CompareGains(Suggestion firstSuggestion, Suggestion secondSuggestion)
    {
        return secondSuggestion.GainScore.CompareTo(firstSuggestion.GainScore);
    }

    private static bool UserHasSkill(List<UserSkill> userSkills, string skillName)
    {
        foreach (var userSkill in userSkills)
        {
            if (string.Equals(userSkill.Skill?.Name, skillName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static List<Suggestion> IdentifyGaps(IReadOnlyList<SkillGroup> skillGroups, List<UserSkill> userSkills, int totalWeight)
    {
        var missingSkillsSuggestions = new List<Suggestion>();

        foreach (var skillGroup in skillGroups)
        {
            double groupScore = ComputeGroupScore(skillGroup, userSkills);

            if (groupScore > HighSkillCoverageThreshold)
            {
                continue;
            }

            Suggestion? bestSuggestion = null;

            foreach (var skill in skillGroup.Skills)
            {
                if (UserHasSkill(userSkills, skill.Name))
                {
                    continue;
                }

                double gain = ComputeGain(skillGroup, groupScore, totalWeight);

                bestSuggestion = new Suggestion
                {
                    SkillName = skill.Name,
                    GroupName = skillGroup.GroupName,
                    GainScore = gain,
                };
                break;
            }

            if (bestSuggestion is not null)
            {
                missingSkillsSuggestions.Add(bestSuggestion);
            }
        }

        missingSkillsSuggestions.Sort(CompareGains);

        if (missingSkillsSuggestions.Count > MaxSuggestions)
        {
            missingSkillsSuggestions = missingSkillsSuggestions.GetRange(0, MaxSuggestions);
        }

        return missingSkillsSuggestions;
    }

    public async Task<RoleResult> CalculateForRoleAsync(int userId, JobRole role, CancellationToken cancellationToken = default)
    {
        var userSkills = await GetUserSkillsAsync(userId, cancellationToken).ConfigureAwait(false);
        var groups = await skillGroupRepository.GetByJobRoleAsync(role, cancellationToken).ConfigureAwait(false);

        int totalWeight = 0;
        foreach (var group in groups)
        {
            totalWeight += group.Weight;
        }

        var groupScores = new List<double>();
        foreach (var group in groups)
        {
            groupScores.Add(ComputeGroupScore(group, userSkills));
        }

        double matchScore = ComputeMatchScore(groups, groupScores);

        var result = new RoleResult
        {
            JobRole = role,
        };

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
        {
            results.Add(await CalculateForRoleAsync(userId, role, cancellationToken).ConfigureAwait(false));
        }

        return results;
    }

    public IReadOnlyList<Suggestion> GetSuggestions(RoleResult result)
    {
        return result.Suggestions;
    }
}
