namespace PussyCats.Library.Domain.Enums;

public static class DeveloperPostParameterTypeMapper
{
    public static DeveloperPostParameterType FromStorageValue(string? value)
    {
        var normalized = Normalize(value);
        return normalized switch
        {
            "mitigationfactor" => DeveloperPostParameterType.MitigationFactor,
            "weighteddistancescoreweight" => DeveloperPostParameterType.WeightedDistanceScoreWeight,
            "jobresumesimilarityscoreweight" => DeveloperPostParameterType.JobResumeSimilarityScoreWeight,
            "preferencescoreweight" => DeveloperPostParameterType.PreferenceScoreWeight,
            "promotionscoreweight" => DeveloperPostParameterType.PromotionScoreWeight,
            "relevantkeyword" => DeveloperPostParameterType.RelevantKeyword,
            _ => DeveloperPostParameterType.Unknown,
        };
    }

    public static string ToStorageValue(DeveloperPostParameterType type)
    {
        return type switch
        {
            DeveloperPostParameterType.MitigationFactor => "mitigation factor",
            DeveloperPostParameterType.WeightedDistanceScoreWeight => "weighted distance score weight",
            DeveloperPostParameterType.JobResumeSimilarityScoreWeight => "job-resume similarity score weight",
            DeveloperPostParameterType.PreferenceScoreWeight => "preference score weight",
            DeveloperPostParameterType.PromotionScoreWeight => "promotion score weight",
            DeveloperPostParameterType.RelevantKeyword => "relevant keyword",
            _ => string.Empty,
        };
    }

    public static string ToDisplayName(DeveloperPostParameterType type)
    {
        return type switch
        {
            DeveloperPostParameterType.MitigationFactor => "Mitigation Factor",
            DeveloperPostParameterType.WeightedDistanceScoreWeight => "Weighted Distance Score Weight",
            DeveloperPostParameterType.JobResumeSimilarityScoreWeight => "Job-Resume Similarity Score Weight",
            DeveloperPostParameterType.PreferenceScoreWeight => "Preference Score Weight",
            DeveloperPostParameterType.PromotionScoreWeight => "Promotion Score Weight",
            DeveloperPostParameterType.RelevantKeyword => "Relevant Keyword",
            _ => "Unknown Parameter",
        };
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Concat(value.ToLowerInvariant().Where(char.IsLetterOrDigit));
    }
}
