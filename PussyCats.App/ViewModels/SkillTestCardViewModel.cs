using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Services;
using PussyCats.Library.Domain;

namespace PussyCats.App.ViewModels;

public partial class SkillTestCardViewModel : DispatchableObservableObject
{
    private const int MinimumRetakeScore = 0;
    private const int MaximumRetakeScore = 100;

    private readonly ISkillTestService skillTestService;
    private readonly UserProfileViewModel userProfileViewModel;
    private SkillTest skillTest;
    private Badge badge;
    private bool isRetakeEnabled;

    public SkillTestCardViewModel(ISkillTestService skillTestService, UserProfileViewModel userProfileViewModel)
        : this(new SkillTest(), skillTestService, userProfileViewModel)
    {
    }

    public SkillTestCardViewModel(
        SkillTest skillTest,
        ISkillTestService skillTestService,
        UserProfileViewModel userProfileViewModel)
    {
        this.skillTest = skillTest;
        this.skillTestService = skillTestService;
        this.userProfileViewModel = userProfileViewModel;
        badge = SimpleModelOperations.AssignTier(skillTest.Score);
    }

    public SkillTest SkillTest
    {
        get => skillTest;
        private set => SetProperty(ref skillTest, value);
    }

    public Badge Badge
    {
        get => badge;
        private set => SetProperty(ref badge, value);
    }

    public bool IsRetakeEnabled
    {
        get => isRetakeEnabled;
        private set => SetProperty(ref isRetakeEnabled, value);
    }

    public async Task LoadCardAsync(CancellationToken ct = default)
    {
        await CheckRetakeEligibleAsync(ct).ConfigureAwait(false);
        UpdateBadge();
    }

    public async Task CheckRetakeEligibleAsync(CancellationToken ct = default)
    {
        IsRetakeEnabled = SkillTest.SkillTestId > 0 &&
            await skillTestService.CanRetakeTestAsync(SkillTest.SkillTestId, ct).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task RetakeAsync(CancellationToken ct)
    {
        if (!IsRetakeEnabled)
        {
            return;
        }

        var newTestScore = Random.Shared.Next(MinimumRetakeScore, MaximumRetakeScore + 1);
        Badge = await skillTestService.SubmitRetakeAsync(SkillTest.SkillTestId, newTestScore, ct).ConfigureAwait(false);

        SkillTest.AchievedDate = DateOnly.FromDateTime(DateTime.Now);
        SkillTest.Score = newTestScore;
        OnPropertyChanged(nameof(SkillTest));

        await CheckRetakeEligibleAsync(ct).ConfigureAwait(false);
        UpdateBadge();
        await userProfileViewModel.RecalculateLevelAsync(ct).ConfigureAwait(false);
    }

    public void UpdateBadge()
    {
        Badge = SimpleModelOperations.AssignTier(SkillTest.Score);
    }
}
