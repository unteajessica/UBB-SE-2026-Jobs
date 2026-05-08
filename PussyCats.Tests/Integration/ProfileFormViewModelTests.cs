using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.SkillTests;
using PussyCats.Library.Repositories.Users;
using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.Integration;

public class ProfileFormViewModelTests
{
    private readonly IUserRepository userRepo = new FakeUserRepository();
    private readonly ISkillTestRepository skillTestRepository= new FakeSkillTestRepository();
    private readonly ICvParsingService cvParsingService = Substitute.For<ICvParsingService>();
    private readonly SessionContext session = new() { UserId = 15 };

    private readonly UserProfileService profileService;
    private readonly ProfileFormViewModel viewModel;

    public ProfileFormViewModelTests()
    {
        profileService = new UserProfileService(userRepo,skillTestRepository);
        viewModel = new ProfileFormViewModel(profileService, cvParsingService, session);
    }

    [Fact]
    public void LoadProfile_UserPassed_MapsUserFieldsIntoFormProperties()
    {
        var user = ValidUser();
        user.Phone = "+40 123456789";

        viewModel.LoadProfile(user);

        viewModel.FirstName.Should().Be("Ada");
        viewModel.LastName.Should().Be("Lovelace");
        viewModel.PhonePrefix.Should().Be("+40");
        viewModel.PhoneNumber.Should().Be("123456789");
        viewModel.ExpectedGraduationYear.Should().Be(2027);
    }

    [Fact]
    public async Task SaveProfileAsync_RequiredFieldsMissing_ReturnsFalseAndShowsInfoBar()
    {
        var saved = await viewModel.SaveProfileAsync();

        saved.Should().BeFalse();
        viewModel.IsInfoBarOpen.Should().BeTrue();
        viewModel.InfoBarMessage.Should().Contain("Please fill in required fields");

        var persisted = await userRepo.GetByIdAsync(15);
        persisted.Should().BeNull();
    }

    [Fact]
    public async Task SaveProfileAsync_ValidData_PersistsTrimmedProfileToRepository()
    {
        viewModel.LoadProfile(ValidUser());
        viewModel.FirstName = " Ada ";
        viewModel.Email = " ADA@EXAMPLE.COM ";
        viewModel.AddSkill("C#");

        var saved = await viewModel.SaveProfileAsync();

        saved.Should().BeTrue();
        var persisted = await userRepo.GetByIdAsync(15);
        persisted.Should().NotBeNull();
        persisted!.FirstName.Should().Be("Ada");
        persisted.Email.Should().Be("ada@example.com");
        persisted.Skills.Should().HaveCount(1);
        viewModel.InfoBarMessage.Should().Be("Profile saved successfully!");
    }

    [Fact]
    public void AddSkill_DuplicateSkill_RejectsAndShowsErrorMessage()
    {
        viewModel.AddSkill("C#");
        viewModel.AddSkill("c#");

        viewModel.Skills.Should().ContainSingle("C#");
        viewModel.InfoBarMessage.Should().Contain("already been added");
    }

    [Fact]
    public void ProcessCvFile_FileParsed_PopulatesFormFromParserResult()
    {
        var parsedUser = ValidUser();
        parsedUser.FirstName = "Grace";
        parsedUser.Skills = [new UserSkill { Skill = new Skill { Name = "COBOL" } }];
        cvParsingService.ParseCvFile("content", "json").Returns(parsedUser);

        viewModel.ProcessCvFile("content", "json");

        viewModel.FirstName.Should().Be("Grace");
        viewModel.Skills.Should().ContainSingle("COBOL");
        viewModel.CvStatusText.Should().Be("CV loaded successfully!");
    }

    private static User ValidUser()
    {
        return new User
        {
            UserId = 15,
            FirstName = "Ada",
            LastName = "Lovelace",
            Age = 22,
            Gender = "Female",
            Email = "ada@example.com",
            Phone = "+40123456789",
            Country = "Romania",
            City = "Cluj-Napoca",
            University = "Babes-Bolyai University",
            Degree = "Computer Science",
            ExpectedGraduationYear = 2027
        };
    }
}