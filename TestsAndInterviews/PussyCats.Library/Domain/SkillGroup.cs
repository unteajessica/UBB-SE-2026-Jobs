using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public class SkillGroup
{
    public int SkillGroupId { get; set; }

    public string GroupName { get; set; } = string.Empty;
    public int Weight { get; set; }
    public JobRole JobRole { get; set; }

    public List<Skill> Skills { get; set; } = new();
}
