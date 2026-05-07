using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class SkillTest
{
    public int SkillTestId { get; set; }

    public int UserId { get; set; }
    [JsonIgnore] public User User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateOnly AchievedDate { get; set; }
}
