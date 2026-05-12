using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;

namespace PussyCats.Tests.Algorithm;

public class RecommendationAlgorithmTests
{
    private readonly RecommendationAlgorithm algorithm = new();

    [Fact]
    public void CalculateCompatibilityScore_PerfectMatch_Returns100()
    {
        var user = BuildUser(locationPreference: "Cluj-Napoca", employmentType: "Full-time", parsedCv: "C# SQL");
        var job = BuildJob(location: "Cluj-Napoca", employmentType: "Full-time", description: "C# SQL", promotionLevel: 100);
        var userSkills = new[]
        {
            BuildUserSkill(skillId: 1, score: 80, name: "C#"),
            BuildUserSkill(skillId: 2, score: 60, name: "SQL"),
        };
        var jobSkills = new[]
        {
            BuildJobSkill(skillId: 1, requiredLevel: 80, name: "C#"),
            BuildJobSkill(skillId: 2, requiredLevel: 60, name: "SQL"),
        };

        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);

        score.Should().Be(100);
    }

    [Fact]
    public void CalculateCompatibilityScore_DefaultEqualWeights_ReturnsApproximateScore()
    {
        var user = BuildUser(locationPreference: "Cluj-Napoca", employmentType: "Part-time", parsedCv: "csharp sql azure");
        var job = BuildJob(location: "Cluj-Napoca", employmentType: "Full-time", description: "csharp docker", promotionLevel: 40);
        var userSkills = new[]
        {
            BuildUserSkill(skillId: 1, score: 80, name: "C#"),
        };
        var jobSkills = new[]
        {
            BuildJobSkill(skillId: 1, requiredLevel: 70, name: "C#"),
            BuildJobSkill(skillId: 2, requiredLevel: 60, name: "Docker"),
        };

        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);

        score.Should().BeApproximately(45.625, 0.0001);
    }

    [Fact]
    public void CalculateScoreBreakdown_ValidInputs_ReturnsComponentScoresAndRoundedOverallScore()
    {
        var user = BuildUser(locationPreference: "Cluj-Napoca", employmentType: "Part-time", parsedCv: "csharp sql azure");
        var job = BuildJob(location: "Cluj-Napoca", employmentType: "Full-time", description: "csharp docker", promotionLevel: 40);
        var userSkills = new[]
        {
            BuildUserSkill(skillId: 1, score: 80, name: "C#"),
        };
        var jobSkills = new[]
        {
            BuildJobSkill(skillId: 1, requiredLevel: 70, name: "C#"),
            BuildJobSkill(skillId: 2, requiredLevel: 60, name: "Docker"),
        };

        var breakdown = algorithm.CalculateScoreBreakdown(user, job, userSkills, jobSkills);

        breakdown.SkillScore.Should().Be(67.5);
        breakdown.KeywordScore.Should().Be(25);
        breakdown.PreferenceScore.Should().Be(50);
        breakdown.PromotionScore.Should().Be(40);
        breakdown.OverallScore.Should().Be(45.6);
    }

    [Fact]
    public void CalculateCompatibilityScore_SkillsIdsDifferButNamesMatch_Returns100()
    {
        var user = BuildUser(parsedCv: "python", locationPreference: "Remote", employmentType: "Full-time");
        var job = BuildJob(description: "python", location: "Remote", employmentType: "Full-time", promotionLevel: 100);
        var userSkills = new[]
        {
            BuildUserSkill(skillId: 10, score: 75, name: "Python"),
        };
        var jobSkills = new[]
        {
            BuildJobSkill(skillId: 99, requiredLevel: 75, name: "Python"),
        };

        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);

        score.Should().Be(100);
    }

    [Fact]
    public void CalculateScoreBreakdown_ComponentsExceedThreshold_ClampsToPercentageRange()
    {
        var user = BuildUser(parsedCv: "go", locationPreference: "Remote", employmentType: "Contract");
        var job = BuildJob(description: "go", location: "Remote", employmentType: "Contract", promotionLevel: 150);
        var userSkills = new[]
        {
            BuildUserSkill(skillId: 1, score: 100, name: "Go"),
        };
        var jobSkills = new[]
        {
            BuildJobSkill(skillId: 1, requiredLevel: 100, name: "Go"),
        };

        var breakdown = algorithm.CalculateScoreBreakdown(user, job, userSkills, jobSkills);

        breakdown.PromotionScore.Should().Be(100);
        breakdown.OverallScore.Should().Be(100);
    }

    [Fact]
    public void CalculateCompatibilityScore_JobHasNoRequiredSkills_ReturnsZeroSkillScore()
    {
        var user = BuildUser(parsedCv: "java", locationPreference: "Remote", employmentType: "Full-time");
        var job = BuildJob(description: "java", location: "Remote", employmentType: "Full-time", promotionLevel: 100);

        var breakdown = algorithm.CalculateScoreBreakdown(user, job, [], []);

        breakdown.SkillScore.Should().Be(0);
        breakdown.OverallScore.Should().Be(75);
    }

    [Fact(Skip = "Dynamic weights deferred per MergePlan section 8.")]
    public void RecommendationAlgorithm_InteractionHistoryProvided_ReweightsComponents()
    {
        _ = new RecommendationAlgorithm(new object(), new object());
    }

    private static User BuildUser(
        string parsedCv,
        string locationPreference,
        string employmentType)
    {
        return new User
        {
            UserId = 1,
            ParsedCv = parsedCv,
            LocationPreference = locationPreference,
            PreferredEmploymentType = employmentType,
        };
    }

    private static Job BuildJob(
        string description,
        string location,
        string employmentType,
        int promotionLevel)
    {
        return new Job
        {
            JobId = 1,
            JobDescription = description,
            Location = location,
            EmploymentType = employmentType,
            PromotionLevel = promotionLevel,
        };
    }

    private static UserSkill BuildUserSkill(int skillId, int score, string name)
    {
        return new UserSkill
        {
            User = new User { UserId = 1 },
            Score = score,
            Skill = new Skill { SkillId = skillId, Name = name },
        };
    }

    private static JobSkill BuildJobSkill(int skillId, int requiredLevel, string name)
    {
        return new JobSkill
        {
            RequiredLevel = requiredLevel,
            Skill = new Skill { SkillId = skillId, Name = name },
        };
    }
}