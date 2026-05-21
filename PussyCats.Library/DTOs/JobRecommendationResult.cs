using System.Text.Json.Serialization;
using PussyCats.Library.Domain;

namespace PussyCats.Library.DTOs;

public sealed class JobRecommendationResult
{
    public required Job Job { get; init; }
    public required Company Company { get; init; }
    public double CompatibilityScore { get; init; }
    public int? DisplayRecommendationId { get; init; }
    public IReadOnlyList<string> TopSkillLabels { get; init; } = new List<string>();
    public IReadOnlyList<string> AllSkillLabels { get; init; } = new List<string>();

    // Display-only helpers — [JsonIgnore] keeps them out of the wire format so a
    // partially-bound card (e.g. only Job.JobId set when posting back from a form)
    // doesn't trip on null nav properties during serialization.
    [JsonIgnore]
    public string JobTitleLine
    {
        get
        {
            var title = Job?.JobTitle?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(title))
            {
                return title.Length > 80 ? title[..80] + "..." : title;
            }

            var trimmedDescription = Job?.JobDescription?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(trimmedDescription))
            {
                return string.Empty;
            }

            var firstLine = GetFirstLine(trimmedDescription);
            return firstLine.Length > 80 ? firstLine[..80] + "..." : firstLine;
        }
    }

    [JsonIgnore]
    public string DescriptionExcerpt => BuildExcerpt(Job?.JobDescription ?? string.Empty, 200);

    [JsonIgnore]
    public string LocationEmploymentLine => $"{Job?.Location} - {Job?.EmploymentType}";

    [JsonIgnore]
    public string MatchScoreDisplay => $"{CompatibilityScore:0.#}%";

    [JsonIgnore]
    public string MatchLineLabel => $"Match: {MatchScoreDisplay}";

    [JsonIgnore]
    public string ContactLine => $"{Company?.Email} - {Company?.Phone}";

    public static string BuildExcerpt(string description, int maxChars)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return string.Empty;
        }

        var trimmed = description.Trim();
        if (trimmed.Length <= maxChars)
        {
            return trimmed;
        }

        return trimmed[..maxChars].TrimEnd() + "...";
    }

    public static IReadOnlyList<string> TakeTopSkills(IEnumerable<JobSkill> jobSkills, int count = 3)
    {
        var skillLabels = new List<string>();
        var index = 0;
        foreach (var jobSkill in jobSkills)
        {
            if (index >= count)
            {
                break;
            }

            var skillName = jobSkill.Skill?.Name ?? $"Skill #{jobSkill.Skill.SkillId}";
            skillLabels.Add($"{skillName} (min {jobSkill.RequiredLevel})");
            index++;
        }

        return skillLabels;
    }

    private static string GetFirstLine(string text)
    {
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return lines.Length == 0 ? text : lines[0];
    }
}
