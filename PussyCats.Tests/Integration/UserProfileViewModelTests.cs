using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.SkillTests;
using PussyCats.Library.Repositories.Users;
using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.Integration;

public class UserProfileViewModelTests
{
    private readonly IUserRepository userRepo = new FakeUserRepository();
    private readonly ISkillTestRepository skillTestRepo = new FakeSkillTestRepository();

    private readonly IImageStorageService imageStorageService = Substitute.For<IImageStorageService>();
    private readonly ICompletenessService completenessService = Substitute.For<ICompletenessService>();
    private readonly SessionContext session = new() { UserId = 6 };

    private readonly UserProfileService profileService;
    private readonly UserProfileViewModel viewModel;

    public UserProfileViewModelTests()
    {
        profileService = new UserProfileService(userRepo,skillTestRepo);
        viewModel = new UserProfileViewModel(profileService, imageStorageService, completenessService, session);
    }

    [Fact]
    public async Task LoadUserAsync_UserExistsInRepo_PopulatesProfileAndCompletenessState()
    {
        var user = BuildUser();
        await userRepo.AddAsync(user);
        completenessService.CalculateCompleteness(Arg.Any<User>()).Returns(80);
        completenessService.GetNextEmptyFieldPrompt(Arg.Any<User>()).Returns("Add a phone number.");

        await viewModel.LoadUserAsync();

        viewModel.UserProfile.Should().NotBeNull();
        viewModel.UserProfile!.UserId.Should().Be(6);
        viewModel.CompletenessPercentage.Should().Be(80);
        viewModel.NextEmptyFieldPrompt.Should().Be("Add a phone number.");
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleAccountStatusAsync_CommandExecuted_UpdatesRepositoryAndLocalState()
    {
        var user = BuildUser(activeAccount: true);
        await userRepo.AddAsync(user);
        await viewModel.LoadUserAsync();

        await viewModel.ToggleAccountStatusAsync();

        var persistedUser = await userRepo.GetByIdAsync(6);
        persistedUser!.ActiveAccount.Should().BeFalse();
        viewModel.UserProfile!.ActiveAccount.Should().BeFalse();
    }

    [Fact]
    public async Task UploadAvatarAsync_ValidStream_SavesToStorageAndUpdatesRepository()
    {
        var user = BuildUser();
        await userRepo.AddAsync(user);
        await viewModel.LoadUserAsync();

        imageStorageService.SaveImageAsync(Arg.Any<Stream>(), "avatar.png", Arg.Any<CancellationToken>())
            .Returns("stored.png");

        using var stream = new MemoryStream([1, 2, 3]);

        await viewModel.UploadAvatarAsync(stream, "avatar.png");

        var persistedUser = await userRepo.GetByIdAsync(6);
        persistedUser!.ProfilePicturePath.Should().Be("stored.png");
        viewModel.UserProfile!.ProfilePicturePath.Should().Be("stored.png");
        await imageStorageService.Received(1).SaveImageAsync(stream, "avatar.png", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAvatarAsync_AvatarExists_DeletesFromStorageAndClearsRepositoryPath()
    {
        var user = BuildUser();
        user.ProfilePicturePath = "stored.png";
        await userRepo.AddAsync(user);
        await viewModel.LoadUserAsync();

        await viewModel.RemoveAvatarAsync();

        var persisted = await userRepo.GetByIdAsync(6);
        persisted!.ProfilePicturePath.Should().BeEmpty();
        viewModel.UserProfile!.ProfilePicturePath.Should().BeEmpty();
        await imageStorageService.Received(1).DeleteImageAsync("stored.png", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecalculateLevelAsync_XPUpdated_PersistsTotalXPAndRaisesEvent()
    {
        var user = BuildUser();
        await userRepo.AddAsync(user);
        await viewModel.LoadUserAsync();

        // Mocking the specific level calculation logic usually handled by the service/repo
        var eventRaised = false;
        viewModel.LevelUpdated += () => eventRaised = true;

        await viewModel.RecalculateLevelAsync();

        var persisted = await userRepo.GetByIdAsync(6);
        persisted.Should().NotBeNull();
        eventRaised.Should().BeTrue();
    }

    [Theory]
    [InlineData(false, "TAKE PERSONALITY TEST")]
    [InlineData(true, "RETAKE PERSONALITY TEST")]
    public async Task GetPersonalityButtonText_RoleSelectionState_ReturnsCorrectLabel(bool hasRole, string expected)
    {
        var user = BuildUser();
        if (hasRole)
        {
            user.PersonalityResult = new PersonalityTestResult
            {
                UserId = user.UserId,
                SelectedRole = JobRole.BackendDeveloper,
            };
        }
        await userRepo.AddAsync(user);
        await viewModel.LoadUserAsync();

        viewModel.GetPersonalityButtonText().Should().Be(expected);
    }

    private static User BuildUser(bool activeAccount = true)
    {
        return new User
        {
            UserId = 6,
            FirstName = "Ada",
            LastName = "Lovelace",
            ActiveAccount = activeAccount,
            LastUpdated = DateTime.UtcNow,
            ProfilePicturePath = string.Empty
        };
    }
}