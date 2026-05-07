using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;

namespace PussyCats.Tests.ViewModels;

public class ProfileFormViewModelTests
{
    private readonly IUserProfileService profileService = Substitute.For<IUserProfileService>();
    private readonly ICvParsingService cvParsingService = Substitute.For<ICvParsingService>();
    private readonly SessionContext session = new() { UserId = 15 };

    [Fact]
    public void LoadProfile_maps_user_fields_into_form_properties()
    {
        var user = ValidUser();
        user.Phone = "+40 123456789";
        var viewModel = CreateViewModel();

        viewModel.LoadProfile(user);

        viewModel.FirstName.Should().Be("Ada");
        viewModel.LastName.Should().Be("Lovelace");
        viewModel.PhonePrefix.Should().Be("+40");
        viewModel.PhoneNumber.Should().Be("123456789");
        viewModel.ExpectedGraduationYear.Should().Be(2027);
    }

    [Fact]
    public async Task SaveProfileAsync_returns_false_and_shows_infobar_when_required_fields_missing()
    {
        var viewModel = CreateViewModel();

        var saved = await viewModel.SaveProfileAsync();

        saved.Should().BeFalse();
        viewModel.IsInfoBarOpen.Should().BeTrue();
        viewModel.InfoBarMessage.Should().Contain("Please fill in required fields");
        await profileService.DidNotReceiveWithAnyArgs().SaveAsync(default, default!, default);
    }

    [Fact]
    public async Task SaveProfileAsync_persists_trimmed_profile_when_valid()
    {
        var viewModel = CreateViewModel();
        viewModel.LoadProfile(ValidUser());
        viewModel.FirstName = " Ada ";
        viewModel.Email = " ADA@EXAMPLE.COM ";
        viewModel.AddSkill("C#");

        var saved = await viewModel.SaveProfileAsync();

        saved.Should().BeTrue();
        await profileService.Received(1).SaveAsync(
            15,
            Arg.Is<User>(user => user.FirstName == "Ada" && user.Email == "ada@example.com" && user.Skills.Count == 1),
            Arg.Any<CancellationToken>());
        viewModel.InfoBarMessage.Should().Be("Profile saved successfully!");
    }

    [Fact]
    public void AddSkill_rejects_duplicates_case_insensitively()
    {
        var viewModel = CreateViewModel();

        viewModel.AddSkill("C#");
        viewModel.AddSkill("c#");

        viewModel.Skills.Should().ContainSingle("C#");
        viewModel.InfoBarMessage.Should().Contain("already been added");
    }

    [Fact]
    public void ProcessCvFile_populates_form_from_parser_result()
    {
        var parsedUser = ValidUser();
        parsedUser.FirstName = "Grace";
        parsedUser.Skills =
        [
            new UserSkill { Skill = new Skill { Name = "COBOL" } },
        ];
        cvParsingService.ParseCvFile("content", "json").Returns(parsedUser);
        var viewModel = CreateViewModel();

        viewModel.ProcessCvFile("content", "json");

        viewModel.FirstName.Should().Be("Grace");
        viewModel.Skills.Should().ContainSingle("COBOL");
        viewModel.CvStatusText.Should().Be("CV loaded successfully!");
    }

    private ProfileFormViewModel CreateViewModel()
    {
        return new ProfileFormViewModel(profileService, cvParsingService, session);
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
            ExpectedGraduationYear = 2027,
            LastUpdated = DateTime.UtcNow,
        };
    }
}
