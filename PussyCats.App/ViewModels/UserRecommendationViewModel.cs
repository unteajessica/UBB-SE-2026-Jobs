using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.App.ViewModels;

public sealed class UserRecommendationViewModel : DispatchableObservableObject
{
    private readonly IUserRecommendationService service;
    private readonly SessionContext session;
    private readonly RelayCommand refreshCommand;
    private readonly RelayCommand likeCommand;
    private readonly RelayCommand dismissCommand;
    private readonly RelayCommand undoCommand;
    private readonly RelayCommand openFiltersCommand;
    private readonly RelayCommand applyFiltersCommand;
    private readonly RelayCommand resetFiltersCommand;
    private readonly RelayCommand openDetailCommand;
    private readonly RelayCommand closeDetailCommand;
    private readonly UserMatchmakingFilters appliedFilters = UserMatchmakingFilters.Empty();

    private JobRecommendationResult? currentJob;
    private bool isLoading;
    private string errorMessage = string.Empty;
    private bool isFilterOpen;
    private bool isDetailOpen;
    private bool canUndo;
    private bool undoConsumedThisSession;
    private UndoSnapshot? undoSnapshot;
    private string draftLocation = string.Empty;

    public UserRecommendationViewModel(IUserRecommendationService service, SessionContext session)
    {
        this.service = service;
        this.session = session;

        foreach (var label in EmploymentTypeOptions)
        {
            DraftEmploymentSelections.Add(new FilterCheckItem(label));
        }

        foreach (var label in ExperienceLevelOptions)
        {
            DraftExperienceSelections.Add(new FilterCheckItem(label));
        }

        refreshCommand = new RelayCommand(ExecuteRefreshCommand, CanRefresh);
        likeCommand = new RelayCommand(ExecuteLikeCommand, CanAct);
        dismissCommand = new RelayCommand(ExecuteDismissCommand, CanAct);
        undoCommand = new RelayCommand(ExecuteUndoCommand, CanUndoAction);
        openFiltersCommand = new RelayCommand(OpenFilters);
        applyFiltersCommand = new RelayCommand(ExecuteApplyFiltersCommand);
        resetFiltersCommand = new RelayCommand(ResetDraftFilters);
        openDetailCommand = new RelayCommand(ExpandCard, CanOpenDetail);
        closeDetailCommand = new RelayCommand(CollapseCard);
    }

    public event Action<string>? ErrorOccurred;

    public static IReadOnlyList<string> EmploymentTypeOptions { get; } =
    [
        "Full-time", "Part-time", "Internship", "Volunteer", "Remote", "Hybrid",
    ];

    public static IReadOnlyList<string> ExperienceLevelOptions { get; } =
    [
        "Internship", "Entry", "MidSenior", "Director", "Executive",
    ];

    public ObservableCollection<FilterCheckItem> DraftEmploymentSelections { get; } = new();
    public ObservableCollection<FilterCheckItem> DraftExperienceSelections { get; } = new();
    public ObservableCollection<SkillFilterItem> DraftSkillSelections { get; } = new();

    public ICommand RefreshCommand => refreshCommand;
    public ICommand LikeCommand => likeCommand;
    public ICommand DismissCommand => dismissCommand;
    public ICommand UndoCommand => undoCommand;
    public ICommand OpenFiltersCommand => openFiltersCommand;
    public ICommand ApplyFiltersCommand => applyFiltersCommand;
    public ICommand ResetFiltersCommand => resetFiltersCommand;
    public ICommand OpenDetailCommand => openDetailCommand;
    public ICommand CloseDetailCommand => closeDetailCommand;

    public JobRecommendationResult? CurrentJob
    {
        get => currentJob;
        private set
        {
            if (SetProperty(ref currentJob, value))
            {
                OnPropertyChanged(nameof(HasCard));
                OnPropertyChanged(nameof(ShowEmptyDeck));
                RaiseCommands();
            }
        }
    }

    public bool IsLoading
    {
        get => isLoading;
        private set
        {
            if (SetProperty(ref isLoading, value))
            {
                OnPropertyChanged(nameof(ShowEmptyDeck));
                RaiseCommands();
            }
        }
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set
        {
            if (SetProperty(ref errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
                OnPropertyChanged(nameof(ShowEmptyDeck));
            }
        }
    }

    public bool IsFilterOpen
    {
        get => isFilterOpen;
        set => SetProperty(ref isFilterOpen, value);
    }

    public bool IsDetailOpen
    {
        get => isDetailOpen;
        set
        {
            if (SetProperty(ref isDetailOpen, value))
            {
                openDetailCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool CanUndo
    {
        get => canUndo;
        private set
        {
            if (SetProperty(ref canUndo, value))
            {
                undoCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string DraftLocation
    {
        get => draftLocation;
        set => SetProperty(ref draftLocation, value);
    }

    public bool HasCard => CurrentJob is not null;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool ShowEmptyDeck => !IsLoading && CurrentJob is null && string.IsNullOrEmpty(ErrorMessage) && IsCandidateSession;

    private bool IsCandidateSession => session.Mode == AppMode.Candidate && ViewModelSupport.ResolveUserId(session) > 0;

    public void ExpandCard() => IsDetailOpen = true;
    public void CollapseCard() => IsDetailOpen = false;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await LoadDeckAsync(ct).ConfigureAwait(false);
    }

    public Task LoadRecommendationsAsync(CancellationToken ct = default)
    {
        return LoadDeckAsync(ct);
    }

    public async Task LikeAsync(CancellationToken ct = default)
    {
        var job = CurrentJob;
        if (job is null || !IsCandidateSession)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var userId = ViewModelSupport.ResolveUserId(session);
            var matchId = await service.ApplyLikeAsync(userId, job, ct).ConfigureAwait(false);
            if (!undoConsumedThisSession)
            {
                undoSnapshot = new UndoSnapshot
                {
                    Card = job,
                    WasApply = true,
                    MatchId = matchId,
                };
                CanUndo = true;
            }

            IsDetailOpen = false;
            await AdvanceAfterActionAsync(userId, ct).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            ReportError(exception.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DismissAsync(CancellationToken ct = default)
    {
        var job = CurrentJob;
        if (job is null || !IsCandidateSession)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var userId = ViewModelSupport.ResolveUserId(session);
            var dismissedRecommendationId = await service.ApplyDismissAsync(userId, job, ct).ConfigureAwait(false);
            if (!undoConsumedThisSession)
            {
                undoSnapshot = new UndoSnapshot
                {
                    Card = job,
                    WasApply = false,
                    RecommendationId = dismissedRecommendationId,
                };
                CanUndo = true;
            }

            IsDetailOpen = false;
            await AdvanceAfterActionAsync(userId, ct).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            ReportError(exception.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task UndoAsync(CancellationToken ct = default)
    {
        var snapshot = undoSnapshot;
        if (snapshot is null || !CanUndo)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            if (snapshot.WasApply && snapshot.MatchId is { } matchId)
            {
                await service.UndoLikeAsync(matchId, snapshot.Card.DisplayRecommendationId, ct).ConfigureAwait(false);
            }
            else if (!snapshot.WasApply && snapshot.RecommendationId is { } recommendationId)
            {
                await service.UndoDismissAsync(recommendationId, snapshot.Card.DisplayRecommendationId, ct).ConfigureAwait(false);
            }

            CurrentJob = snapshot.Card;
            undoSnapshot = null;
            undoConsumedThisSession = true;
            CanUndo = false;
        }
        catch (Exception exception)
        {
            ReportError(exception.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ApplyFiltersAsync(CancellationToken ct = default)
    {
        appliedFilters.EmploymentTypes.Clear();
        foreach (var item in DraftEmploymentSelections.Where(item => item.IsChecked))
        {
            appliedFilters.EmploymentTypes.Add(item.Label);
        }

        appliedFilters.ExperienceLevels.Clear();
        foreach (var item in DraftExperienceSelections.Where(item => item.IsChecked))
        {
            appliedFilters.ExperienceLevels.Add(item.Label);
        }

        appliedFilters.LocationSubstring = DraftLocation.Trim();
        appliedFilters.SkillIds.Clear();
        foreach (var skill in DraftSkillSelections.Where(skill => skill.IsChecked))
        {
            appliedFilters.SkillIds.Add(skill.SkillId);
        }

        IsFilterOpen = false;
        await LoadDeckAsync(ct).ConfigureAwait(false);
    }

    public void ResetDraftFilters()
    {
        foreach (var item in DraftEmploymentSelections)
        {
            item.IsChecked = false;
        }

        foreach (var item in DraftExperienceSelections)
        {
            item.IsChecked = false;
        }

        foreach (var item in DraftSkillSelections)
        {
            item.IsChecked = false;
        }

        DraftLocation = string.Empty;
    }

    public void SetSkillFilterOptions(IEnumerable<SkillFilterItem> skillOptions)
    {
        DraftSkillSelections.Clear();
        foreach (var skillOption in skillOptions)
        {
            DraftSkillSelections.Add(skillOption);
        }
    }

    private async Task LoadDeckAsync(CancellationToken ct)
    {
        if (!IsCandidateSession)
        {
            ReportError("Candidate session is not available.");
            CurrentJob = null;
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var userId = ViewModelSupport.ResolveUserId(session);
            var next = await service.GetNextCardAsync(userId, appliedFilters, ct).ConfigureAwait(false)
                ?? await service.RecalculateTopCardIgnoringCooldownAsync(userId, appliedFilters, ct).ConfigureAwait(false);

            CurrentJob = next;
            if (next is null)
            {
                ErrorMessage = string.Empty;
            }
        }
        catch (Exception exception)
        {
            ReportError(exception.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AdvanceAfterActionAsync(int userId, CancellationToken ct)
    {
        CurrentJob = await service.GetNextCardAsync(userId, appliedFilters, ct).ConfigureAwait(false);
    }

    private void RaiseCommands()
    {
        refreshCommand.NotifyCanExecuteChanged();
        likeCommand.NotifyCanExecuteChanged();
        dismissCommand.NotifyCanExecuteChanged();
        undoCommand.NotifyCanExecuteChanged();
        openDetailCommand.NotifyCanExecuteChanged();
    }

    private void ReportError(string message)
    {
        ErrorMessage = message;
        ErrorOccurred?.Invoke(message);
    }

    private bool CanAct() => CurrentJob is not null && !IsLoading && IsCandidateSession;
    private bool CanRefresh() => !IsLoading;
    private bool CanUndoAction() => CanUndo && !IsLoading;
    private bool CanOpenDetail() => CurrentJob is not null;
    private void OpenFilters() => IsFilterOpen = true;
    private void ExecuteRefreshCommand() => _ = LoadRecommendationsAsync();
    private void ExecuteLikeCommand() => _ = LikeAsync();
    private void ExecuteDismissCommand() => _ = DismissAsync();
    private void ExecuteUndoCommand() => _ = UndoAsync();
    private void ExecuteApplyFiltersCommand() => _ = ApplyFiltersAsync();

    private sealed class UndoSnapshot
    {
        public required JobRecommendationResult Card { get; init; }
        public bool WasApply { get; init; }
        public int? MatchId { get; init; }
        public int? RecommendationId { get; init; }
    }
}
