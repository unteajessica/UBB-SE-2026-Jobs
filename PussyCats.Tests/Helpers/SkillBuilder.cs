using PussyCats.Library.Domain;

namespace PussyCats.Tests.Fakes;

public class SkillBuilder
{
    private int skillId = 1;
    private string name = "C#";
    private string category = "Programming";

    public SkillBuilder WithId(int id)
    {
        skillId = id;
        return this;
    }

    public SkillBuilder WithName(string value)
    {
        name = value;
        return this;
    }

    public SkillBuilder WithCategory(string value)
    {
        category = value;
        return this;
    }

    public Skill Build() => new()
    {
        SkillId = skillId,
        Name = name,
        Category = category,
    };
}
