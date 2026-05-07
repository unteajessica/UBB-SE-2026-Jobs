using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class JobSkill
{
    public int JobId { get; set; }
    [JsonIgnore] public Job Job { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public int RequiredLevel { get; set; }
}
