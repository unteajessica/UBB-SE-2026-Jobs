using System.Globalization;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;

namespace PussyCats_App.Services.RecommendationAlgorithm;

public class RecommendationAlgorithm : IRecommendationAlgorithm
{
    private const int ScoreComponentCount = 4;
    private const int PreferenceCriterionCount = 2;
    private const int BreakdownScoreDecimalPlaces = 1;
    private const double ScoreMinimum = 0.0;
    private const double PercentageScale = 100.0;
    private const double ScoreMaximum = PercentageScale;
    private const double DefaultWeight = PercentageScale / ScoreComponentCount;
    private const double DefaultMitigationFactor = 2.0;
    private const double MinimumMitigationFactor = 1.0;
    private const double DefaultKeywordValue = 1.0;
    private const double KeywordSocialSignalScale = 0.1;
    private const double MaximumKeywordValue = 5.0;

    private readonly bool hasCachedInteractionParameters;
    private readonly double cachedSkillWeight;
    private readonly double cachedResumeWeight;
    private readonly double cachedPreferenceWeight;
    private readonly double cachedPromotionWeight;
    private readonly double cachedMitigationFactor;
    private readonly IReadOnlyDictionary<string, int> cachedKeywordSignalByKeyword;

    public RecommendationAlgorithm()
    {
        hasCachedInteractionParameters = false;
        cachedSkillWeight = DefaultWeight;
        cachedResumeWeight = DefaultWeight;
        cachedPreferenceWeight = DefaultWeight;
        cachedPromotionWeight = DefaultWeight;
        cachedMitigationFactor = DefaultMitigationFactor;
        cachedKeywordSignalByKeyword = new Dictionary<string, int>(StringComparer.Ordinal);
    }

    public RecommendationAlgorithm(object sqlPostRepository, object sqlInteractionRepository)
        : this()
    {
        _ = sqlPostRepository;
        _ = sqlInteractionRepository;
        throw new NotSupportedException(
            "Dynamic recommendation weights depend on the deferred Developer/Interaction feed; see MergePlan.md section 8.");
    }

    public double CalculateCompatibilityScore(
        User user,
        Job job,
        IReadOnlyList<UserSkill> userSkills,
        IReadOnlyList<JobSkill> jobSkills)
    {
        if (hasCachedInteractionParameters)
        {
            return CalculateCompatibilityScoreCore(
                user,
                job,
                userSkills,
                jobSkills,
                cachedSkillWeight,
                cachedResumeWeight,
                cachedPreferenceWeight,
                cachedPromotionWeight,
                cachedMitigationFactor,
                cachedKeywordSignalByKeyword);
        }

        return CalculateCompatibilityScoreCore(
            user,
            job,
            userSkills,
            jobSkills,
            cachedSkillWeight,
            cachedResumeWeight,
            cachedPreferenceWeight,
            cachedPromotionWeight,
            cachedMitigationFactor,
            cachedKeywordSignalByKeyword);
    }

    public CompatibilityBreakdown CalculateScoreBreakdown(
        User user,
        Job job,
        IReadOnlyList<UserSkill> userSkills,
        IReadOnlyList<JobSkill> jobSkills)
    {
        return CalculateBreakdownCore(
            user,
            job,
            userSkills,
            jobSkills,
            cachedSkillWeight,
            cachedResumeWeight,
            cachedPreferenceWeight,
            cachedPromotionWeight,
            cachedMitigationFactor,
            cachedKeywordSignalByKeyword);
    }

    private static CompatibilityBreakdown CalculateBreakdownCore(
        User user,
        Job job,
        IReadOnlyList<UserSkill> userSkills,
        IReadOnlyList<JobSkill> jobSkills,
        double skillWeight,
        double resumeWeight,
        double preferenceWeight,
        double promotionWeight,
        double mitigationFactor,
        IReadOnlyDictionary<string, int> keywordSignalByKeyword)
    {
        var skillScore = CalculateSkillScore(userSkills, jobSkills, mitigationFactor);
        var keywordScore = CalculateKeywordScore(user.ParsedCv, job.JobDescription, keywordSignalByKeyword);
        var preferenceScore = CalculatePreferenceScore(user, job);
        var promotionScore = CalculatePromotionScore(job);

        var finalScore = CalculateWeightedScore(
            skillScore,
            keywordScore,
            preferenceScore,
            promotionScore,
            skillWeight,
            resumeWeight,
            preferenceWeight,
            promotionWeight);

        return new CompatibilityBreakdown
        {
            SkillScore = Math.Round(skillScore, BreakdownScoreDecimalPlaces),
            KeywordScore = Math.Round(keywordScore, BreakdownScoreDecimalPlaces),
            PreferenceScore = Math.Round(preferenceScore, BreakdownScoreDecimalPlaces),
            PromotionScore = Math.Round(promotionScore, BreakdownScoreDecimalPlaces),
            OverallScore = Clamp(Math.Round(finalScore, BreakdownScoreDecimalPlaces), ScoreMinimum, ScoreMaximum),
        };
    }

    private static double CalculateCompatibilityScoreCore(
        User user,
        Job job,
        IReadOnlyList<UserSkill> userSkills,
        IReadOnlyList<JobSkill> jobSkills,
        double skillWeight,
        double resumeWeight,
        double preferenceWeight,
        double promotionWeight,
        double mitigationFactor,
        IReadOnlyDictionary<string, int> keywordSignalByKeyword)
    {
        var skillScore = CalculateSkillScore(userSkills, jobSkills, mitigationFactor);
        var keywordScore = CalculateKeywordScore(user.ParsedCv, job.JobDescription, keywordSignalByKeyword);
        var preferenceScore = CalculatePreferenceScore(user, job);
        var promotionScore = CalculatePromotionScore(job);

        var finalScore = CalculateWeightedScore(
            skillScore,
            keywordScore,
            preferenceScore,
            promotionScore,
            skillWeight,
            resumeWeight,
            preferenceWeight,
            promotionWeight);

        return Clamp(finalScore, ScoreMinimum, ScoreMaximum);
    }

    private static double CalculateWeightedScore(
        double skillScore,
        double keywordScore,
        double preferenceScore,
        double promotionScore,
        double skillWeight,
        double resumeWeight,
        double preferenceWeight,
        double promotionWeight)
    {
        return ((skillScore * skillWeight) +
                (keywordScore * resumeWeight) +
                (preferenceScore * preferenceWeight) +
                (promotionScore * promotionWeight)) / PercentageScale;
    }

    private static Dictionary<int, double> TransformUserSkillsToDictionaryOfIdAndScore(IReadOnlyList<UserSkill> userSkills)
    {
        Dictionary<int, double> dictionaryOfIdAndScore = new Dictionary<int, double>();
        foreach (var userSkill in userSkills)
        {
            dictionaryOfIdAndScore[userSkill.Skill.SkillId] = userSkill.Score;
        }

        return dictionaryOfIdAndScore;
    }

    private static Dictionary<string, double> TransformUserSkillsToDictionaryOfSkillNameAndScore(IReadOnlyList<UserSkill> userSkills)
    {
        Dictionary<string, double> dictionaryOfSkillNameAndScore = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var userSkill in userSkills)
        {
            var skillName = GetSkillName(userSkill.Skill);
            if (!string.IsNullOrWhiteSpace(skillName))
            {
                dictionaryOfSkillNameAndScore[skillName] = userSkill.Score;
            }
        }

        return dictionaryOfSkillNameAndScore;
    }

    private static double CalculateSkillScore(
        IReadOnlyList<UserSkill> userSkills,
        IReadOnlyList<JobSkill> jobSkills,
        double mitigationFactor)
    {
        if (jobSkills.Count == 0)
        {
            return ScoreMinimum;
        }

        var userScoreBySkillId = TransformUserSkillsToDictionaryOfIdAndScore(userSkills);
        var userScoreBySkillName = TransformUserSkillsToDictionaryOfSkillNameAndScore(userSkills);

        var penaltySum = ScoreMinimum;

        foreach (var requiredSkill in jobSkills)
        {
            var targetScore = requiredSkill.RequiredLevel;
            var skillName = GetSkillName(requiredSkill.Skill);

            var userScore = userScoreBySkillId.TryGetValue(requiredSkill.Skill.SkillId, out var skillScoreFoundById)
                ? skillScoreFoundById
                : userScoreBySkillName.TryGetValue(skillName, out var skillScoreFoundByName)
                    ? skillScoreFoundByName
                    : ScoreMinimum;

            var difference = userScore - targetScore;
            var asymmetricPenalty = difference > 0
                ? difference / mitigationFactor
                : -difference;

            penaltySum += asymmetricPenalty;
        }

        var averagePenalty = penaltySum / jobSkills.Count;
        var score = ScoreMaximum - averagePenalty;

        return Math.Max(ScoreMinimum, score);
    }

    private static double CalculateKeywordScore(
        string userResume,
        string jobDescription,
        IReadOnlyDictionary<string, int> keywordSignalByKeyword)
    {
        var userTerms = TokenizeDistinct(userResume);
        var jobTerms = TokenizeDistinct(jobDescription);

        var union = new HashSet<string>(userTerms, StringComparer.Ordinal);
        union.UnionWith(jobTerms);
        if (union.Count == 0)
        {
            return ScoreMinimum;
        }

        var intersection = new HashSet<string>(userTerms, StringComparer.Ordinal);
        intersection.IntersectWith(jobTerms);

        var intersectionScore = SumKeywordValues(intersection, keywordSignalByKeyword);
        var unionScore = SumKeywordValues(union, keywordSignalByKeyword);

        if (unionScore <= 0)
        {
            return ScoreMinimum;
        }

        var ratio = intersectionScore / unionScore;
        return Clamp(ratio * PercentageScale, ScoreMinimum, ScoreMaximum);
    }

    private static double CalculatePreferenceScore(User user, Job job)
    {
        var matches = 0;

        if (string.Equals(user.LocationPreference, job.Location, StringComparison.OrdinalIgnoreCase))
        {
            matches++;
        }

        if (string.Equals(user.PreferredEmploymentType, job.EmploymentType, StringComparison.OrdinalIgnoreCase))
        {
            matches++;
        }

        return (matches / (double)PreferenceCriterionCount) * PercentageScale;
    }

    private static double CalculatePromotionScore(Job job)
    {
        return Clamp(job.PromotionLevel, ScoreMinimum, ScoreMaximum);
    }

    private static double KeywordValue(
        string keyword,
        IReadOnlyDictionary<string, int> keywordSignalByKeyword)
    {
        var normalizedKeyword = NormalizeText(keyword).Trim();

        if (string.IsNullOrWhiteSpace(normalizedKeyword))
        {
            return DefaultKeywordValue;
        }

        keywordSignalByKeyword.TryGetValue(normalizedKeyword, out var socialSignal);

        var rawValue = DefaultKeywordValue + (KeywordSocialSignalScale * socialSignal);
        return Math.Min(MaximumKeywordValue, Math.Abs(rawValue));
    }

    private static double SumKeywordValues(
        IEnumerable<string> keywords,
        IReadOnlyDictionary<string, int> keywordSignalByKeyword)
    {
        var sum = 0d;
        foreach (var keyword in keywords)
        {
            sum += KeywordValue(keyword, keywordSignalByKeyword);
        }

        return sum;
    }

    private static HashSet<string> GetUniqueTokensFromString(string text)
    {
        var tokens = new HashSet<string>(StringComparer.Ordinal);
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            tokens.Add(word);
        }

        return tokens;
    }

    private static HashSet<string> TokenizeDistinct(string text)
    {
        var normalized = NormalizeText(text);

        return GetUniqueTokensFromString(normalized);
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var characters = text.ToLowerInvariant().ToCharArray();
        for (var characterIndex = 0; characterIndex < characters.Length; characterIndex++)
        {
            if (!char.IsLetterOrDigit(characters[characterIndex]) && !char.IsWhiteSpace(characters[characterIndex]))
            {
                characters[characterIndex] = ' ';
            }
        }

        return new string(characters);
    }

    private static bool TryParseDouble(string text, out double value)
    {
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    private static string GetSkillName(Skill? skill)
    {
        return skill?.Name ?? string.Empty;
    }
}
