using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Services;
using PussyCats.Library.Domain;

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

    public async Task LoadPublicProfileAsync(int userId, CancellationToken ct = default)
    {
        try
        {
            IsAvailable = await userProfileService.IsProfileAvailableAsync(userId, ct).ConfigureAwait(false);
            if (!IsAvailable)
            {
                Profile = null;
                Tests = new List<SkillTest>();
                return;
            }

            Profile = await userProfileService.GetProfileAsync(userId, ct).ConfigureAwait(false);
            Tests = (await userProfileService.GetSkillTestsForUserAsync(userId, ct).ConfigureAwait(false)).ToList();
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Error loading public profile: {exception.Message}";
        }
    }

    public string GetAvailabilityMessage() => IsAvailable ? string.Empty : "Profile Unavailable";
}
