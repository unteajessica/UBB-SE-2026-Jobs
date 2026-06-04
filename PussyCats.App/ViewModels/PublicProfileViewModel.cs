using CommunityToolkit.Mvvm.ComponentModel;
using PussyCats.App.Services.TI;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.UserProfileService;

namespace PussyCats.App.ViewModels;

public class PublicProfileViewModel : DispatchableObservableObject
{
    private readonly IUserProfileService userProfileService;
    private readonly ITiTestService tiTestService;
    private User? profile;
    private List<SkillDisplay> tests = new();
    private bool isAvailable;
    private string errorMessage = string.Empty;

    public PublicProfileViewModel(IUserProfileService userProfileService, ITiTestService tiTestService)
    {
        this.userProfileService = userProfileService;
        this.tiTestService = tiTestService;
    }

    public User? Profile
    {
        get => profile;
        private set => SetProperty(ref profile, value);
    }

    // Simplified TI skill-test results (test name + percentage) for the profile.
    public List<SkillDisplay> Tests
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
                Tests = new List<SkillDisplay>();
                return;
            }

            Profile = await userProfileService.GetProfileAsync(userId, cancellationToken);
            Tests = await LoadTiSkillResultsAsync(userId);
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Error loading public profile: {exception.Message}";
        }
    }

    private async Task<List<SkillDisplay>> LoadTiSkillResultsAsync(int userId)
    {
        var results = new List<SkillDisplay>();

        foreach (var test in await tiTestService.GetAllAsync())
        {
            var attempt = await tiTestService.GetAttemptByUserAndTestAsync(userId, test.Id);
            if (!ViewModelSupport.IsTiAttemptCompleted(attempt))
            {
                continue;
            }

            var questions = await tiTestService.GetQuestionsByTestIdAsync(test.Id);
            float maxPossibleScore = questions.Sum(question => question.QuestionScore);

            results.Add(new SkillDisplay
            {
                Name = test.Title,
                Score = ViewModelSupport.TiPercentage(attempt!.Score, maxPossibleScore),
            });
        }

        return results;
    }

    public string GetAvailabilityMessage() => IsAvailable ? string.Empty : "Profile Unavailable";
}
