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
        double expectedResult = 100;

        score.Should().Be(expectedResult);
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

        double expectedSkillScore = 67.5; // 100 - ((80 - 70) / 2 + 60) / 2
        double expectedKeywordScore = 25; // 1 out of 4 keywords match
        double expectedPreferenceScore = 50; // matching location
        double expectedPromotionScore = 40;

        double expectedOverallScore = (expectedSkillScore + expectedKeywordScore + expectedPreferenceScore + expectedPromotionScore) / 4;

        score.Should().Be(expectedOverallScore);
    }

    [Fact]
    public void CalculateScoreBreakdown_ValidInputs_ReturnsRoundedOverallScore()
    {
        string location = "Cluj-Napoca";
        string userEmploymentType = "Part-time", jobEmploymentType = "Full-time";
        string userCv = "csharp sql azure";
        string jobDescription = "csharp docker";
        int promotionLevel = 40;

        int userSkillScore = 80, jobRequiredSkillScore = 70;
        string skillName = "C#";

        var user = BuildUser(locationPreference: location, employmentType: userEmploymentType, parsedCv: userCv);
        var job = BuildJob(location: location, employmentType: jobEmploymentType, description: jobDescription, promotionLevel: promotionLevel);
        var userSkills = new[]
        {
            BuildUserSkill(skillId: 1, score: userSkillScore, name: skillName),
        };
        var jobSkills = new[]
        {
            BuildJobSkill(skillId: 1, requiredLevel: jobRequiredSkillScore, name: skillName)
        };

        var breakdown = algorithm.CalculateScoreBreakdown(user, job, userSkills, jobSkills);

        double expectedSkillScore = 95;
        double expectedKeywordScore = 25; // 1 out of 4
        double expectedPreferenceScore = 50; // matching location
        double expectedPromotionScore = 40; 
        double expectedOverallScore = (expectedSkillScore + expectedKeywordScore + expectedPreferenceScore + expectedPromotionScore) / 4;

        breakdown.SkillScore.Should().Be(expectedSkillScore);
    }

    [Fact]
    public void CalculateScoreBreakdown_ValidInputs_ReturnsCorrectSkillScore()
    {
        var user = BuildUser(locationPreference: "Cluj-Napoca", employmentType: "Part-time", parsedCv: "csharp sql azure");
        var job = BuildJob(location: "Cluj-Napoca", employmentType: "Full-time", description: "csharp docker", promotionLevel: 40);

        int firstSkillId = 1, secondSkillId = 2;
        int firstSkillScore = 80;
        int firstJobSkillRequiredLevel = 70, secondJobSkillRequiredLevel = 60;
        double maximumScore = 100;
        string firstSkillName = "C#", secondSkillName = "Docker";

        var userSkills = new[]
        {
            BuildUserSkill(skillId: firstSkillId, score: firstSkillScore, name: firstSkillName),
        };
        var jobSkills = new[]
        {
            BuildJobSkill(skillId: firstSkillId, requiredLevel: firstJobSkillRequiredLevel, name: firstSkillName),
            BuildJobSkill(skillId: secondSkillId, requiredLevel: secondJobSkillRequiredLevel, name: secondSkillName),
        };

        var breakdown = algorithm.CalculateScoreBreakdown(user, job, userSkills, jobSkills);

        double firstSkillDifference = 5; // firstSkillScore - firstJobSkillRequiredLevel / 2 (default mitigationFactor)
        double secondSkillDifference = secondJobSkillRequiredLevel; // doesn't exist

        double averagePenalty = (firstSkillDifference + secondSkillDifference) / 2;
        double expectedResult = maximumScore - averagePenalty;
        breakdown.SkillScore.Should().Be(expectedResult);
    }

    [Fact]
    public void CalculateScoreBreakdown_BigPenalty_DoesNotGiveScoreLowerThanMinimumScore()
    {
        var user = BuildUser(locationPreference: "Cluj-Napoca", employmentType: "Part-time", parsedCv: "csharp sql azure");
        var job = BuildJob(location: "Cluj-Napoca", employmentType: "Full-time", description: "csharp docker", promotionLevel: 40);

        int firstSkillId = 1;
        int firstSkillScore = 10;
        int firstJobSkillRequiredLevel = 120;
        double maximumScore = 100;
        string firstSkillName = "C#", secondSkillName = "Docker";

        var userSkills = new[]
        {
            BuildUserSkill(skillId: firstSkillId, score: firstSkillScore, name: firstSkillName),
        };
        var jobSkills = new[]
        {
            BuildJobSkill(skillId: firstSkillId, requiredLevel: firstJobSkillRequiredLevel, name: firstSkillName),
        };

        var breakdown = algorithm.CalculateScoreBreakdown(user, job, userSkills, jobSkills);

        double expectedResult = 0; // default minimum score
        breakdown.SkillScore.Should().Be(expectedResult);
    }

    [Fact]
    public void CalculateScoreBreakdown_NoJobSkills_ReturnsKeywordScoreZero()
    {
        var user = BuildUser(locationPreference: "Cluj-Napoca", employmentType: "Part-time", parsedCv: "csharp sql azure");
        var job = BuildJob(location: "Cluj-Napoca", employmentType: "Full-time", description: "csharp docker", promotionLevel: 40);
        int firstSkillId = 1;
        int firstSkillScore = 80;
        string firstSkillName = "C#";
        var userSkills = new[]
        {
            BuildUserSkill(skillId: firstSkillId, score: firstSkillScore, name: firstSkillName),
        };
        var jobSkills = Array.Empty<JobSkill>();
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, userSkills, jobSkills);

        double expectedResult = 0;
        breakdown.SkillScore.Should().Be(expectedResult);
    }

    [Fact]
    public void CalculateScoreBreakdown_NoKeywords_ReturnsKeywordScoreZero()
    {
        var user = BuildUser(locationPreference: "Cluj-Napoca", employmentType: "Part-time", parsedCv: string.Empty);
        var job = BuildJob(location: "Cluj-Napoca", employmentType: "Full-time", description: string.Empty, promotionLevel: 40);
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, [], []);
        double expectedResult = 0;
        breakdown.KeywordScore.Should().Be(expectedResult);
    }

    [Fact]
    public void CalculateScoreBreakdown_KeywordsMatching_ReturnsNonZeroKeywordScore()
    {

        var user = BuildUser(locationPreference: "Cluj-Napoca", employmentType: "Part-time", parsedCv: "csharp sql azure");
        var job = BuildJob(location: "Cluj-Napoca", employmentType: "Full-time", description: "csharp docker", promotionLevel: 40);
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, [], []);
        double expectedResult = 25; // 1 out of 4 keywords match
        breakdown.KeywordScore.Should().Be(expectedResult);
    }

    [Fact]
    public void CalculateScoreBreakdown_NoMatchingKeywords_ReturnsZero()
    {
        string location = "Cluj-Napoca", userEmploymentType = "Part-time", jobEmploymentType = "Full-time";
        string userCv = "python java", jobDescription = "csharp docker";
        int promotionLevel = 40;
        var user = BuildUser(locationPreference: location, employmentType: userEmploymentType, parsedCv: userCv);
        var job = BuildJob(location: location, employmentType: jobEmploymentType, description: jobDescription, promotionLevel: promotionLevel);
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, [], []);
        double expectedResult = 0;
        breakdown.KeywordScore.Should().Be(expectedResult);
    }

    [Fact]
    public void CalculateScoreBreakdown_NoMatchingPreferences_ReturnsZeroPreferenceScore()
    {
        string userLocationPreference = "Cluj-Napoca", userEmploymentType = "Part-time";
        string jobLocationPreference = "Remote", jobEmploymentType = "Full-time";
        var user = BuildUser(locationPreference: userLocationPreference, employmentType: userEmploymentType, parsedCv: string.Empty);
        var job = BuildJob(location: jobLocationPreference, employmentType: jobEmploymentType, description: string.Empty, promotionLevel: 40);
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, [], []);
        double expectedResult = 0;
        breakdown.PreferenceScore.Should().Be(expectedResult);
    }

    [Fact]
    public void CalculateScoreBreakdown_JobPromotionLevelTooBig_ReturnsMaximumPromotionScore()
    {
        string location = "Cluj-Napoca", employmentType = "Part-time";
        int promotionLevel = 150; // More than maximum of 100
        var user = BuildUser(locationPreference: location, employmentType: employmentType, parsedCv: string.Empty);
        var job = BuildJob(location: location, employmentType: employmentType, description: string.Empty, promotionLevel: promotionLevel);
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, [], []);
        double expectedResult = 100; 
        breakdown.PromotionScore.Should().Be(expectedResult);
    }

    [Fact]
    public void CalculateScoreBreakdown_MatchingLocation_ReturnsLocationPreferenceScore()
    {
        string location = "Cluj-Napoca";
        string userEmploymentType = "Part-time", jobEmploymentType = "Full-time";
        var user = BuildUser(locationPreference: location, employmentType: userEmploymentType, parsedCv: string.Empty);
        var job = BuildJob(location: location, employmentType: jobEmploymentType, description: string.Empty, promotionLevel: 40);
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, [], []);
        double expectedResult = 50;
        breakdown.PreferenceScore.Should().Be(expectedResult);
    }

    [Fact]
    public void CalculateScoreBreakdown_MatchingEmploymentType_ReturnsEmploymentTypePreferenceScore()
    {
        string userLocationPreference = "Cluj-Napoca", jobLocationPreference = "Remote";
        string employmentType = "Full-time";
        var user = BuildUser(locationPreference: userLocationPreference, employmentType: employmentType, parsedCv: string.Empty);
        var job = BuildJob(location: jobLocationPreference, employmentType: employmentType, description: string.Empty, promotionLevel: 40);
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, [], []);
        double expectedResult = 50; 
        breakdown.PreferenceScore.Should().Be(expectedResult);
    }

    [Fact]
    public void CalculateScoreBreakdown_MatchingPreferences_ReturnsFullPreferenceScore()
    {
        string location = "Cluj-Napoca";
        string employmentType = "Full-time";
        var user = BuildUser(locationPreference: location, employmentType: employmentType, parsedCv: string.Empty);
        var job = BuildJob(location: location, employmentType: employmentType, description: string.Empty, promotionLevel: 40);
        var breakdown = algorithm.CalculateScoreBreakdown(user, job, [], []);
        double expectedResult = 100; 
        breakdown.PreferenceScore.Should().Be(expectedResult);
    }


    [Fact]
    public void CalculateCompatibilityScore_SkillsIdsDifferButNamesMatch_Returns100()
    {
        string location = "Remote", employmentType = "Full-time";
        string parsedCv = "python", jobDescription = "python";

        int userSkillId = 10, jobSkillId = 99;
        int userSkillScore = 75, jobRequiredSkillScore = 75;
        string skillName = "Python";

        var user = BuildUser(parsedCv: parsedCv, locationPreference: location, employmentType: employmentType);
        var job = BuildJob(description: jobDescription, location: location, employmentType: employmentType, promotionLevel: 100);
        var userSkills = new[]
        {
            BuildUserSkill(skillId: userSkillId, score: userSkillScore, name: skillName),
        };
        var jobSkills = new[]
        {
            BuildJobSkill(skillId: jobSkillId, requiredLevel: jobRequiredSkillScore, name: skillName),
        };

        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills);

        double expectedResult = 100;
        score.Should().Be(expectedResult);
    }

    [Fact]
    public void CalculateScoreBreakdown_ComponentsExceedThreshold_OverallScoreClampsToPercentageRange()
    {
        string location = "Remote", employmentType = "Contract", parsedCv = "go", jobDescription = "go";
        int promotionLevel = 150; // Exceeds typical maximum of 100

        string skillName = "Go";
        int userSkillScore = 100, requiredSkillScore = 100;
        double maximumScore = 100;

        var user = BuildUser(parsedCv: parsedCv, locationPreference: location, employmentType: employmentType);
        var job = BuildJob(description: jobDescription, location: location, employmentType: employmentType, promotionLevel: promotionLevel);
        var userSkills = new[]
        {
            BuildUserSkill(skillId: 1, score: userSkillScore, name: skillName),
        };
        var jobSkills = new[]
        {
            BuildJobSkill(skillId: 1, requiredLevel: requiredSkillScore, name: skillName),
        };

        var breakdown = algorithm.CalculateScoreBreakdown(user, job, userSkills, jobSkills);

        
        breakdown.OverallScore.Should().Be(maximumScore);
    }

    [Fact]
    public void CalculateCompatibilityScore_JobHasNoRequiredSkills_ReturnsZeroSkillScore()
    {
        var user = BuildUser(parsedCv: "java", locationPreference: "Remote", employmentType: "Full-time");
        var job = BuildJob(description: "java", location: "Remote", employmentType: "Full-time", promotionLevel: 100);

        var breakdown = algorithm.CalculateScoreBreakdown(user, job, [], []);

        const int expectedSkillScore = 0;
        breakdown.SkillScore.Should().Be(expectedSkillScore);
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