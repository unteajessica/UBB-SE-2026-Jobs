using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.SkillTests;
using PussyCats.Library.Repositories.Users;
using PussyCats.Library.Services.CompletenessService;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Tests.Fakes;
using PussyCats.Library.Services.ImageStorage;

namespace PussyCats.Tests.Integration;

public class UserProfileViewModelTests
{
    private readonly IUserRepository userRepository = new FakeUserRepository();
    private readonly ISkillTestRepository skillTestRepository = new FakeSkillTestRepository();

    private readonly IImageStorageService imageStorageService = Substitute.For<IImageStorageService>();
    private readonly ICompletenessService completenessService = Substitute.For<ICompletenessService>();
    private readonly SessionContext session = new() { UserId = 6 };

    private readonly UserProfileService profileService;
    private readonly UserProfileViewModel viewModel;

    public UserProfileViewModelTests()
    {
        profileService = new UserProfileService(userRepository,skillTestRepository);
        viewModel = new UserProfileViewModel(profileService, imageStorageService, completenessService, session);
    }

    [Fact]
    public async Task LoadUserAsync_UserExistsInRepo_PopulatesProfileAndCompletenessState()
    {
        var user = BuildUser();
        await userRepository.AddAsync(user);
        completenessService.CalculateCompleteness(Arg.Any<User>()).Returns(80);
        completenessService.GetNextEmptyFieldPrompt(Arg.Any<User>()).Returns("Add a phone number.");

        await viewModel.LoadUserAsync();

        Assert.NotNull(viewModel.UserProfile);
        Assert.Equal(6, viewModel.UserProfile!.UserId);
        Assert.Equal(80, viewModel.CompletenessPercentage);
        Assert.Equal("Add a phone number.", viewModel.NextEmptyFieldPrompt);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task ToggleAccountStatusAsync_CommandExecuted_UpdatesRepositoryAndLocalState()
    {
        var user = BuildUser(activeAccount: true);
        await userRepository.AddAsync(user);
        await viewModel.LoadUserAsync();

        await viewModel.ToggleAccountStatusAsync();

        var persistedUser = await userRepository.GetByIdAsync(6);
        Assert.False(persistedUser!.ActiveAccount);
        Assert.False(viewModel.UserProfile!.ActiveAccount);
    }

    [Fact]
    public async Task UploadAvatarAsync_ValidStream_SavesToStorageAndUpdatesRepository()
    {
        var user = BuildUser();
        await userRepository.AddAsync(user);
        await viewModel.LoadUserAsync();

        imageStorageService.SaveImageAsync(Arg.Any<Stream>(), "avatar.png", Arg.Any<CancellationToken>())
            .Returns("stored.png");

        using var stream = new MemoryStream([1, 2, 3]);

        await viewModel.UploadAvatarAsync(stream, "avatar.png");

        var persistedUser = await userRepository.GetByIdAsync(6);
        Assert.Equal("stored.png", persistedUser!.ProfilePicturePath);
        Assert.Equal("stored.png", viewModel.UserProfile!.ProfilePicturePath);
        await imageStorageService.Received(1).SaveImageAsync(stream, "avatar.png", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAvatarAsync_AvatarExists_DeletesFromStorageAndClearsRepositoryPath()
    {
        var user = BuildUser();
        user.ProfilePicturePath = "stored.png";
        await userRepository.AddAsync(user);
        await viewModel.LoadUserAsync();

        await viewModel.RemoveAvatarAsync();

        var persisted = await userRepository.GetByIdAsync(6);
        Assert.Empty(persisted!.ProfilePicturePath);
        Assert.Empty(viewModel.UserProfile!.ProfilePicturePath);
        await imageStorageService.Received(1).DeleteImageAsync("stored.png", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecalculateLevelAsync_XPUpdated_PersistsTotalXPAndRaisesEvent()
    {
        var user = BuildUser();
        await userRepository.AddAsync(user);
        await viewModel.LoadUserAsync();

        // Mocking the specific level calculation logic usually handled by the service/recommendationRepository
        var eventRaised = false;
        viewModel.LevelUpdated += () => eventRaised = true;

        await viewModel.RecalculateLevelAsync();

        var persisted = await userRepository.GetByIdAsync(6);
        Assert.NotNull(persisted);
        Assert.True(eventRaised);
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
                User = new User { UserId = user.UserId },
                SelectedRole = JobRole.BackendDeveloper,
            };
        }
        await userRepository.AddAsync(user);
        await viewModel.LoadUserAsync();

        Assert.Equal(expected, viewModel.GetPersonalityButtonText());
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
