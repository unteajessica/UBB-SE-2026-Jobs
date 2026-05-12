using PussyCats.Library.Domain;

namespace PussyCats.Tests.Helpers;

public class SkillTestBuilder
{
    private int skillTestId = 1;
    private int userId = 1;
    private string name = "C# Fundamentals";
    private int score = 80;
    private DateOnly achievedDate = DateOnly.FromDateTime(DateTime.UtcNow);

    public SkillTestBuilder WithId(int id)
    {
        skillTestId = id;
        return this;
    }

    public SkillTestBuilder ForUser(int id)
    {
        userId = id;
        return this;
    }

    public SkillTestBuilder WithName(string value)
    {
        name = value;
        return this;
    }

    public SkillTestBuilder WithScore(int value)
    {
        score = value;
        return this;
    }

    public SkillTestBuilder WithAchievedDate(DateOnly value)
    {
        achievedDate = value;
        return this;
    }

    public SkillTest Build() => new()
    {
        SkillTestId = skillTestId,
        User = new User { UserId = userId },
        Name = name,
        Score = score,
        AchievedDate = achievedDate,
    };
}
