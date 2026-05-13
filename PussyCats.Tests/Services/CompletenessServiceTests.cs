using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Helpers;
using PussyCats_App.Services.CompletenessService;

namespace PussyCats.Tests.Services;

public class CompletenessServiceTests
{
    private readonly CompletenessService service = new();

    [Fact]
    public void CalculateCompleteness_UserIsNull_ReturnsZero()
    {
        User user = null;
        int expectedCompleteness = 0;

        service.CalculateCompleteness(user).Should().Be(expectedCompleteness);
    }

    [Fact]
    public void CalculateCompleteness_UserHasNoFieldsFilled_ReturnsZero()
    {
        var user = new User();
        int expectedCompleteness = 0;

        service.CalculateCompleteness(user).Should().Be(expectedCompleteness);
    }

    [Fact]
    public void CalculateCompleteness_AllFieldsFilled_ReturnsOneHundred()
    {
        var completedUser = BuildFullyFilledUser();
        int expectedCompletenessScore = 100;

        service.CalculateCompleteness(completedUser).Should().Be(expectedCompletenessScore);
    }

    [Fact]
    public void CalculateCompleteness_SingleFieldFilled_ReturnsFivePercent()
    {
        const int expectedPercentage = 5; //1 of 21 fileds ~ 4.76% rounds up to 5
        const string firstName = "Ada";

        var user = new User { FirstName = firstName };

        service.CalculateCompleteness(user).Should().Be(expectedPercentage);
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_UserIsNull_ReturnsEmptyString()
    {
        User user = null;

        service.GetNextEmptyFieldPrompt(user).Should().BeEmpty();
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_UserIsBlank_ReturnsFirstNamePrompt()
    {
        User user = new User();
        const string expectedPrompt = "First Name";

        service.GetNextEmptyFieldPrompt(user).Should().Contain(expectedPrompt);
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_ProfileIsFullyFilled_ReturnsCompleteMessage()
    {
        var fullyFilledUser = BuildFullyFilledUser();
        const string expectedMessage = "Your profile is 100% complete!";

        service.GetNextEmptyFieldPrompt(fullyFilledUser).Should().Be(expectedMessage);
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_SomeFieldsFilled_ReturnsNextFieldInOrder()
    {
        var user = new User
        {
            FirstName = "Ada",
            LastName = "Lovelace",
        };
        const string nextFieldExpected = "Age";

        service.GetNextEmptyFieldPrompt(user).Should().Contain(nextFieldExpected);
    }

    [Fact]
    public void CalculateCompleteness_PersonalityRoleMissing_ReturnsLessThenOneHundred()
    {
        var user = BuildFullyFilledUser();
        user.PersonalityResult!.SelectedRole = null;
        const int maximumCompletenessScore = 100;

        service.CalculateCompleteness(user).Should().BeLessThan(maximumCompletenessScore);

    }

    [Fact]
    public void CalculateCompleteness_AgeIsZero_ReturnsZero()
    {
        var user = new User { Age = 0 };

        service.CalculateCompleteness(user).Should().Be(0);
    }

    [Fact]
    public void GetNextEmpryFieldPrompt_SomeFieldsFilled_ReturnsCorrectNextPercentage()
    {
        const string firstName = "Ada";
        const string lastname = "Lovelance";
        const int expectedNextPercentage = 14; // this would bring filled count to 3 out of 21 => 14%
        User user = new User { FirstName = "Ada", LastName = "Lovelance" };

        service.GetNextEmptyFieldPrompt(user).Should().Contain($"{ expectedNextPercentage}%");

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
