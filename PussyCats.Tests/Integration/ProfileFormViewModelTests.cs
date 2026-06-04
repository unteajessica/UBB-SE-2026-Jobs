using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Library.Repositories.SkillTests;
using PussyCats.Library.Repositories.Users;
using PussyCats.Tests.Fakes;
using PussyCats.Library.Services.CvParsing;
using System.Text;

namespace PussyCats.Tests.Integration;

public class ProfileFormViewModelTests
{
    private readonly IUserRepository userRepository = new FakeUserRepository();
    private readonly ISkillTestRepository skillTestRepository= new FakeSkillTestRepository();
    private readonly ICvParsingService cvParsingService = Substitute.For<ICvParsingService>();
    private readonly SessionContext session = new() { UserId = 15 };

    private readonly UserProfileService profileService;
    private readonly ProfileFormViewModel viewModel;

    public ProfileFormViewModelTests()
    {
        profileService = new UserProfileService(userRepository, skillTestRepository, cvParsingService);
        viewModel = new ProfileFormViewModel(profileService, session);
    }

    [Fact]
    public void LoadProfile_UserPassed_MapsUserFieldsIntoFormProperties()
    {
        var user = ValidUser();
        user.Phone = "+40 123456789";

        viewModel.LoadProfile(user);

        Assert.Equal("Ada", viewModel.FirstName);
        Assert.Equal("Lovelace", viewModel.LastName);
        Assert.Equal("+40", viewModel.PhonePrefix);
        Assert.Equal("123456789", viewModel.PhoneNumber);
        Assert.Equal(2027, viewModel.ExpectedGraduationYear);
    }

    [Fact]
    public async Task SaveProfileAsync_RequiredFieldsMissing_ReturnsFalseAndShowsInfoBar()
    {
        var saved = await viewModel.SaveProfileAsync();

        Assert.False(saved);
        Assert.True(viewModel.IsInfoBarOpen);
        Assert.Contains("Please fill in required fields", viewModel.InfoBarMessage);

        var persisted = await userRepository.GetByIdAsync(15);
        Assert.Null(persisted);
    }

    [Fact]
    public async Task SaveProfileAsync_ValidData_PersistsTrimmedProfileToRepository()
    {
        viewModel.LoadProfile(ValidUser());
        viewModel.FirstName = " Ada ";
        viewModel.Email = " ADA@EXAMPLE.COM ";
        viewModel.AddSkill("C#");

        var saved = await viewModel.SaveProfileAsync();

        Assert.True(saved);
        var persisted = await userRepository.GetByIdAsync(15);
        Assert.NotNull(persisted);
        Assert.Equal("Ada", persisted!.FirstName);
        Assert.Equal("ada@example.com", persisted.Email);
        Assert.Equal(1, persisted.Skills.Count());
        Assert.Equal("Profile saved successfully!", viewModel.InfoBarMessage);
    }

    [Fact]
    public void AddSkill_DuplicateSkill_RejectsAndShowsErrorMessage()
    {
        viewModel.AddSkill("C#");
        viewModel.AddSkill("c#");

        Assert.Single(viewModel.Skills);
        Assert.Contains("already been added", viewModel.InfoBarMessage);
    }

    [Fact]
    public async Task ProcessCvFileAsync_FileParsed_PopulatesFormFromParserResult()
    {
        var parsedUser = ValidUser();
        parsedUser.FirstName = "Grace";
        parsedUser.Skills = [new UserSkill { Skill = new Skill { Name = "COBOL" } }];
        cvParsingService.ParseCvFile("content", ".json").Returns(parsedUser);

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
        await viewModel.ProcessCvFileAsync(stream, "cv.json");

        Assert.Equal("Grace", viewModel.FirstName);
        Assert.Single(viewModel.Skills);
        Assert.Equal("CV loaded successfully!", viewModel.CvStatusText);
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
