using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.Library.Domain;

namespace PussyCats.App.ViewModels;

public partial class UserProfileViewModel : DispatchableObservableObject
{
    private readonly IUserProfileService profileService;
    private readonly IImageStorageService imageStorageService;
    private readonly ICompletenessService completenessService;
    private readonly SessionContext session;

    private User? userProfile;
    private bool isLoading;
    private int completenessPercentage;
    private string nextEmptyFieldPrompt = string.Empty;
    private List<string> missingFieldWarnings = new();
    private string errorMessage = string.Empty;
    private string freshnessText = string.Empty;
    private int totalExperiencePoints;

    public UserProfileViewModel(
        IUserProfileService userProfileService,
        IImageStorageService imageStorageService,
        ICompletenessService completenessService,
        SessionContext session)
    {
        profileService = userProfileService;
        this.imageStorageService = imageStorageService;
        this.completenessService = completenessService;
        this.session = session;
    }

    public event Action? LevelUpdated;
    public event Action? PersonalityTestRequested;

    public User? UserProfile
    {
        get => userProfile;
        set => SetProperty(ref userProfile, value);
    }

    public bool IsLoading
    {
        get => isLoading;
        set => SetProperty(ref isLoading, value);
    }

    public int CompletenessPercentage
    {
        get => completenessPercentage;
        set => SetProperty(ref completenessPercentage, value);
    }

    public string NextEmptyFieldPrompt
    {
        get => nextEmptyFieldPrompt;
        set => SetProperty(ref nextEmptyFieldPrompt, value);
    }

    public List<string> MissingFieldWarnings
    {
        get => missingFieldWarnings;
        set => SetProperty(ref missingFieldWarnings, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    public string FreshnessText
    {
        get => freshnessText;
        set => SetProperty(ref freshnessText, value);
    }

    public int TotalExperiencePoints
    {
        get => totalExperiencePoints;
        private set => SetProperty(ref totalExperiencePoints, value);
    }

    public async Task RecalculateLevelAsync(CancellationToken cancellationToken = default)
    {
        if (UserProfile is null)
        {
            return;
        }

        try
        {
            TotalExperiencePoints = await profileService.RecalculateLevelAsync(UserProfile, cancellationToken);
            LevelUpdated?.Invoke();
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Error recalculating user level: {exception.Message}";
        }
    }

    public async Task LoadUserAsync(int? userId = null, CancellationToken cancellationToken = default)
    {
        ErrorMessage = string.Empty;
        IsLoading = true;

        try
        {
            var resolvedUserId = userId ?? ViewModelSupport.ResolveUserId(session);
            UserProfile = await profileService.GetProfileAsync(resolvedUserId, cancellationToken);

            if (UserProfile is not null)
            {
                FreshnessText = ViewModelSupport.BuildFreshnessLabel(UserProfile.LastUpdated);
                CompletenessPercentage = completenessService.CalculateCompleteness(UserProfile);
                NextEmptyFieldPrompt = completenessService.GetNextEmptyFieldPrompt(UserProfile);
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Error loading user profile: {exception.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ToggleAccountStatusAsync(CancellationToken cancellationToken = default)
    {
        if (UserProfile is null)
        {
            return;
        }

        var newStatus = !UserProfile.ActiveAccount;
        await profileService.UpdateAccountStatusAsync(UserProfile.UserId, newStatus, cancellationToken);
        UserProfile.ActiveAccount = newStatus;
        OnPropertyChanged(nameof(UserProfile));
    }

    public async Task UploadAvatarAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        if (UserProfile is null)
        {
            return;
        }

        try
        {
            var newPath = await imageStorageService.SaveImageAsync(fileStream, fileName, cancellationToken);
            await profileService.UpdateProfilePicturePathAsync(UserProfile.UserId, newPath, cancellationToken);
            UserProfile.ProfilePicturePath = newPath;
            OnPropertyChanged(nameof(UserProfile));
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Error uploading avatar: {exception.Message}";
        }
    }

    public async Task RemoveAvatarAsync(CancellationToken cancellationToken = default)
    {
        if (UserProfile is null || string.IsNullOrEmpty(UserProfile.ProfilePicturePath))
        {
            return;
        }

        await imageStorageService.DeleteImageAsync(UserProfile.ProfilePicturePath, cancellationToken);
        await profileService.RemoveProfilePicturePathAsync(UserProfile.UserId, cancellationToken);
        UserProfile.ProfilePicturePath = string.Empty;
        OnPropertyChanged(nameof(UserProfile));
    }

    public string GetPersonalityButtonText()
    {
        return UserProfile?.PersonalityResult?.SelectedRole is null
            ? "TAKE PERSONALITY TEST"
            : "RETAKE PERSONALITY TEST";
    }

    [RelayCommand]
    private void TakePersonalityTest()
    {
        PersonalityTestRequested?.Invoke();
    }
}
