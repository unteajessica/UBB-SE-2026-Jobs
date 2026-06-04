using PussyCats.Library.DTOs;

namespace PussyCats.App.ViewModels;

public class CompatibilityDisplayItem
{
    public required RoleResult Result { get; init; }
    public required string DisplayName { get; init; }
    public double DisplayScore { get; init; }
    public required string DisplayPercentage { get; init; }
}

public class SuggestionDisplayItem
{
    public required string SkillName { get; init; }
    public required string GroupName { get; init; }
    public required string GainDisplay { get; init; }
}
