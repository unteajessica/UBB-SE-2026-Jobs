using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Tests.ViewModels;

public class UserProfileViewModelTests
{
    private readonly IUserProfileService profileService = Substitute.For<IUserProfileService>();
    private readonly IImageStorageService imageStorageService = Substitute.For<IImageStorageService>();
    private readonly ICompletenessService completenessService = Substitute.For<ICompletenessService>();
    private readonly SessionContext session = new() { UserId = 6 };

    [Fact]
    public async Task LoadUserAsync_loads_profile_and_completeness_state()
    {
        var user = BuildUser();
        profileService.GetProfileAsync(6, Arg.Any<CancellationToken>()).Returns(Task.FromResult<User?>(user));
        completenessService.CalculateCompleteness(user).Returns(80);
        completenessService.GetNextEmptyFieldPrompt(user).Returns("Add a phone number.");
        var viewModel = CreateViewModel();

        await viewModel.LoadUserAsync();

        viewModel.UserProfile.Should().BeSameAs(user);
        viewModel.CompletenessPercentage.Should().Be(80);
        viewModel.NextEmptyFieldPrompt.Should().Be("Add a phone number.");
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleAccountStatusAsync_updates_service_and_local_profile()
    {
        var user = BuildUser(activeAccount: true);
        var viewModel = CreateViewModel();
        viewModel.UserProfile = user;

        await viewModel.ToggleAccountStatusAsync();

        await profileService.Received(1).UpdateAccountStatusAsync(6, false, Arg.Any<CancellationToken>());
        user.ActiveAccount.Should().BeFalse();
    }

    [Fact]
    public async Task UploadAvatarAsync_saves_image_and_updates_profile_picture_path()
    {
        var user = BuildUser();
        imageStorageService.SaveImageAsync(Arg.Any<Stream>(), "avatar.png", Arg.Any<CancellationToken>()).Returns("stored.png");
        var viewModel = CreateViewModel();
        viewModel.UserProfile = user;
        using var stream = new MemoryStream([1, 2, 3]);

        await viewModel.UploadAvatarAsync(stream, "avatar.png");

        await imageStorageService.Received(1).SaveImageAsync(stream, "avatar.png", Arg.Any<CancellationToken>());
        await profileService.Received(1).UpdateProfilePicturePathAsync(6, "stored.png", Arg.Any<CancellationToken>());
        user.ProfilePicturePath.Should().Be("stored.png");
    }

    [Fact]
    public async Task RemoveAvatarAsync_deletes_image_and_clears_profile_picture_path()
    {
        var user = BuildUser();
        user.ProfilePicturePath = "stored.png";
        var viewModel = CreateViewModel();
        viewModel.UserProfile = user;

        await viewModel.RemoveAvatarAsync();

        await imageStorageService.Received(1).DeleteImageAsync("stored.png", Arg.Any<CancellationToken>());
        await profileService.Received(1).RemoveProfilePicturePathAsync(6, Arg.Any<CancellationToken>());
        user.ProfilePicturePath.Should().BeEmpty();
    }

    [Fact]
    public async Task RecalculateLevelAsync_updates_total_xp_and_raises_event()
    {
        var user = BuildUser();
        profileService.RecalculateLevelAsync(user, Arg.Any<CancellationToken>()).Returns(Task.FromResult(250));
        var viewModel = CreateViewModel();
        viewModel.UserProfile = user;
        var eventRaised = false;
        viewModel.LevelUpdated += () => eventRaised = true;

        await viewModel.RecalculateLevelAsync();

        viewModel.TotalExperiencePoints.Should().Be(250);
        eventRaised.Should().BeTrue();
    }

    [Theory]
    [InlineData(false, "TAKE PERSONALITY TEST")]
    [InlineData(true, "RETAKE PERSONALITY TEST")]
    public void GetPersonalityButtonText_reflects_whether_role_was_selected(bool hasRole, string expected)
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
        var viewModel = CreateViewModel();
        viewModel.UserProfile = user;

        viewModel.GetPersonalityButtonText().Should().Be(expected);
    }

    private UserProfileViewModel CreateViewModel()
    {
        return new UserProfileViewModel(profileService, imageStorageService, completenessService, session);
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
        };
    }
}
