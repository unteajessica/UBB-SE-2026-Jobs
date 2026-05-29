namespace PussyCats.Library.DTOs;

public sealed class UserMatchmakingFilters
{
    public HashSet<string> EmploymentTypes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ExperienceLevels { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string LocationSubstring { get; set; } = string.Empty;
    public HashSet<int> SkillIds { get; } = new();

    public static UserMatchmakingFilters Empty() => new();
}
