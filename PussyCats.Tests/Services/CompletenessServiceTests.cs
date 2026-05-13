using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class CompletenessServiceTests
{
    private readonly CompletenessService service = new();

    [Fact]
    public void CalculateCompleteness_UserIsNull_ReturnsZero()
    {
        const int expectedCompleteness = 0;
        service.CalculateCompleteness(null).Should().Be(expectedCompleteness);
    }

    [Fact]
    public void CalculateCompleteness_UserHasNoFieldsFilled_ReturnsZero()
    {
        var user = new User();
        const int expectedCompleteness = 0;
        service.CalculateCompleteness(user).Should().Be(expectedCompleteness);
    }

    [Fact]
    public void CalculateCompleteness_AllFieldsFilled_ReturnsOneHundred()
    {
        var user = BuildFullyFilledUser();
        const int expectedCompleteness = 100;

        service.CalculateCompleteness(user).Should().Be(expectedCompleteness);
    }

    [Fact]
    public void CalculateCompleteness_SingleFieldFilled_ReturnsFivePercent()
    {
        // 1 of 21 ~= 5%
        var user = new User { FirstName = "Ada" };

        const int expectedCompleteness = 5;
        service.CalculateCompleteness(user).Should().Be(expectedCompleteness);
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_UserIsNull_ReturnsEmptyString()
    {
        service.GetNextEmptyFieldPrompt(null).Should().BeEmpty();
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_UserIsBlank_ReturnsFirstNamePrompt()
    {
        service.GetNextEmptyFieldPrompt(new User()).Should().Contain("First Name");
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_ProfileIsFullyFilled_ReturnsCompleteMessage()
    {
        var fullyFilledUser = BuildFullyFilledUser();

        service.GetNextEmptyFieldPrompt(fullyFilledUser).Should().Be("Your profile is 100% complete!");
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_SomeFieldsFilled_ReturnsNextFieldInOrder()
    {
        var user = new User
        {
            FirstName = "Ada",
            LastName = "Lovelace",
        };

        service.GetNextEmptyFieldPrompt(user).Should().Contain("Age");
    }

    [Fact]
    public void CalculateCompleteness_PersonalityRoleMissing_ReturnsLessThenOneHundred()
    {
        var user = BuildFullyFilledUser();
        user.PersonalityResult!.SelectedRole = null;
        const int maximumCompletenessScore = 100;

        service.CalculateCompleteness(user).Should().BeLessThan(maximumCompletenessScore);

        user.PersonalityResult.SelectedRole = JobRole.BackendDeveloper;

        service.CalculateCompleteness(user).Should().Be(maximumCompletenessScore);
    }

    private static User BuildFullyFilledUser() => new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        Age = 28,
        Gender = "Female",
        Country = "Romania",
        Phone = "+40000000000",
        Email = "ada@example.com",
        University = "Cambridge",
        ExpectedGraduationYear = 2027,
        GitHub = "github.com/ada",
        LinkedIn = "linkedin.com/in/ada",
        Address = "1 Main St",
        ProfilePicturePath = "pic.png",
        Skills = new List<UserSkill> { new() { Skill = new Skill { SkillId = 1 }, Score = 80 } },
        Motivation = "Build cool things.",
        WorkExperiences = new List<WorkExperience> { new() { Company = "Acme", JobTitle = "Eng" } },
        Projects = new List<Project> { new() { Name = "p1" } },
        ExtraCurricularActivities = new List<ExtraCurricularActivity> { new() { ActivityName = "a1" } },
        PersonalityResult = new PersonalityTestResult { SelectedRole = JobRole.BackendDeveloper },
        WorkModePreference = "Remote",
        LocationPreference = "Cluj-Napoca, Romania",
    };
}
