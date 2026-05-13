using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.Library.Domain;
using PussyCats_App.Services.UserProfileService;

namespace PussyCats.App.ViewModels;

public class PublicProfileViewModel : DispatchableObservableObject
{
    private readonly IUserProfileService userProfileService;
    private User? profile;
    private List<SkillTest> tests = new();
    private bool isAvailable;
    private string errorMessage = string.Empty;

    public PublicProfileViewModel(IUserProfileService userProfileService)
    {
        this.userProfileService = userProfileService;
    }

    public User? Profile
    {
        get => profile;
        private set => SetProperty(ref profile, value);
    }

    public List<SkillTest> Tests
    {
        get => tests;
        private set => SetProperty(ref tests, value);
    }

    public bool IsAvailable
    {
        get => isAvailable;
        private set => SetProperty(ref isAvailable, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public async Task LoadPublicProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            IsAvailable = await userProfileService.IsProfileAvailableAsync(userId, cancellationToken);
            if (!IsAvailable)
            {
                Profile = null;
                Tests = new List<SkillTest>();
                return;
            }

            Profile = await userProfileService.GetProfileAsync(userId, cancellationToken);
            Tests = (await userProfileService.GetSkillTestsForUserAsync(userId, cancellationToken)).ToList();
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Error loading public profile: {exception.Message}";
        }
    }

    public string GetAvailabilityMessage() => IsAvailable ? string.Empty : "Profile Unavailable";
}
