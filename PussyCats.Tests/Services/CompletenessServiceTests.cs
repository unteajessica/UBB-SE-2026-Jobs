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
    public void CalculateCompleteness_returns_zero_for_null_user()
    {
        service.CalculateCompleteness(null).Should().Be(0);
    }

    [Fact]
    public void CalculateCompleteness_returns_zero_for_blank_user()
    {
        var user = new User();

        service.CalculateCompleteness(user).Should().Be(0);
    }

    [Fact]
    public void CalculateCompleteness_returns_one_hundred_when_all_21_fields_filled()
    {
        var user = BuildFullyFilledUser();

        service.CalculateCompleteness(user).Should().Be(100);
    }

    [Fact]
    public void CalculateCompleteness_uses_21_fields_total()
    {
        // 1 of 21 ≈ 5%
        var user = new User { FirstName = "Ada" };

        service.CalculateCompleteness(user).Should().Be(5);
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_returns_empty_for_null_user()
    {
        service.GetNextEmptyFieldPrompt(null).Should().BeEmpty();
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_first_empty_field_is_first_name_for_blank_user()
    {
        service.GetNextEmptyFieldPrompt(new User()).Should().Contain("First Name");
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_returns_complete_message_when_all_filled()
    {
        var user = BuildFullyFilledUser();

        service.GetNextEmptyFieldPrompt(user).Should().Be("Your profile is 100% complete!");
    }

    [Fact]
    public void GetNextEmptyFieldPrompt_walks_through_fields_in_label_order()
    {
        var user = new User
        {
            FirstName = "Ada",
            LastName = "Lovelace",
        };

        service.GetNextEmptyFieldPrompt(user).Should().Contain("Age");
    }

    [Fact]
    public void Case_18_uses_PersonalityResult_SelectedRole_not_PreferredJobRoles()
    {
        // Open item: case 18 deviation — original tracked PreferredJobRoles list,
        // merged tracks PersonalityResult.SelectedRole (a single role).
        var user = BuildFullyFilledUser();
        user.PersonalityResult!.SelectedRole = null;

        service.CalculateCompleteness(user).Should().BeLessThan(100);

        user.PersonalityResult.SelectedRole = JobRole.BackendDeveloper;

        service.CalculateCompleteness(user).Should().Be(100);
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
        Skills = new List<UserSkill> { new() { SkillId = 1, Score = 80 } },
        Motivation = "Build cool things.",
        WorkExperiences = new List<WorkExperience> { new() { Company = "Acme", JobTitle = "Eng" } },
        Projects = new List<Project> { new() { Name = "p1" } },
        ExtraCurricularActivities = new List<ExtraCurricularActivity> { new() { ActivityName = "a1" } },
        PersonalityResult = new PersonalityTestResult { SelectedRole = JobRole.BackendDeveloper },
        WorkModePreference = "Remote",
        LocationPreference = "Cluj-Napoca, Romania",
    };
}
