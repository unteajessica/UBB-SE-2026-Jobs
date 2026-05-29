using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public class Badge
{
    public int BadgeId { get; set; }

    public BadgeTier Tier { get; set; }
    public string IconPath { get; set; } = string.Empty;
    public int ExperiencePointsValue { get; set; }
}
