using PussyCats.Library.Domain;

namespace PussyCats.Tests.Helpers;

public class UserBuilder
{
    private int userId = 1;
    private string firstName = "Ada";
    private string lastName = "Lovelace";
    private int age = 28;
    private string email = "ada@example.com";
    private string city = "Cluj-Napoca";
    private string country = "Romania";
    private int currentLevel = 1;
    private int totalExperiencePoints;
    private bool activeAccount = true;
    private string parsedCv = string.Empty;
    private readonly List<UserSkill> skills = new();
    private PersonalityTestResult? personalityResult;

    public UserBuilder WithId(int id)
    {
        userId = id;
        return this;
    }

    public UserBuilder WithEmail(string value)
    {
        email = value;
        return this;
    }

    public UserBuilder WithName(string first, string last)
    {
        firstName = first;
        lastName = last;
        return this;
    }

    public UserBuilder WithAge(int value)
    {
        age = value;
        return this;
    }

    public UserBuilder WithCity(string value)
    {
        city = value;
        return this;
    }

    public UserBuilder WithCountry(string value)
    {
        country = value;
        return this;
    }

    public UserBuilder WithLevel(int level, int totalXp = 0)
    {
        currentLevel = level;
        totalExperiencePoints = totalXp;
        return this;
    }

    public UserBuilder WithActiveAccount(bool value)
    {
        activeAccount = value;
        return this;
    }

    public UserBuilder WithParsedCv(string value)
    {
        parsedCv = value;
        return this;
    }

    public UserBuilder WithSkills(params (int skillId, int score)[] entries)
    {
        foreach (var (skillId, score) in entries)
        {
            skills.Add(new UserSkill
            {
                User = new User { UserId = userId },
                Skill = new Skill { SkillId = skillId },
                Score = score,
                IsVerified = score > 0,
                AchievedDate = score > 0 ? DateOnly.FromDateTime(DateTime.UtcNow) : null,
            });
        }
        return this;
    }

    public UserBuilder WithPersonalityResult(PersonalityTestResult result)
    {
        personalityResult = result;
        return this;
    }

    public User Build()
    {
        var now = DateTime.UtcNow;
        foreach (var skill in skills)
        {
            skill.User.UserId = userId;
        }
        return new User
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            Age = age,
            Email = email,
            City = city,
            Country = country,
            CurrentLevel = currentLevel,
            TotalExperiencePoints = totalExperiencePoints,
            ActiveAccount = activeAccount,
            ParsedCv = parsedCv,
            Skills = skills,
            PersonalityResult = personalityResult,
            CreatedAt = now,
            LastUpdated = now,
        };
    }
}
