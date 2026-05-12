using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class JobSkill
{
    public Job Job { get; set; } = null!;

    public Skill Skill { get; set; } = null!;

    public int RequiredLevel { get; set; }
}
