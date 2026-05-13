using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats_App.Services.CompanyRecommendationService;
using PussyCats_App.Services.MatchService;

namespace PussyCats.App.ViewModels;

public class CompanyRecommendationViewModel : DispatchableObservableObject
{
    private readonly ICompanyRecommendationService recommendationService;
    private readonly IMatchService matchService;
    private readonly SessionContext session;
    private readonly RelayCommand advanceCommand;
    private readonly RelayCommand skipCommand;
    private readonly RelayCommand undoCommand;
    private readonly RelayCommand refreshCommand;
    private readonly RelayCommand expandCommand;
    private readonly RelayCommand collapseCommand;

    private UserApplicationResult? currentApplicant;
    private CompatibilityBreakdown? scoreBreakdown;
    private bool hasApplicant;
    private bool isExpanded;
    private bool isContactRevealed;
    private bool canUndo;
    private bool isLoading;
    private string statusMessage = string.Empty;
    private UserApplicationResult? lastUndoApplicant;
    private bool undoUsed;

    public CompanyRecommendationViewModel(
        ICompanyRecommendationService recommendationService,
        IMatchService matchService,
        SessionContext session)
    {
        this.recommendationService = recommendationService;
        this.matchService = matchService;
        this.session = session;

        advanceCommand = new RelayCommand(ExecuteAdvanceCommand, CanAdvanceOrSkip);
        skipCommand = new RelayCommand(ExecuteSkipCommand, CanAdvanceOrSkip);
        undoCommand = new RelayCommand(ExecuteUndoCommand, CanUndoAction);
        refreshCommand = new RelayCommand(ExecuteRefreshCommand);
        expandCommand = new RelayCommand(ExecuteExpandCommand, CanExpandCard);
        collapseCommand = new RelayCommand(CollapseCard);
    }

    public event Action<string>? ErrorOccurred;

    public UserApplicationResult? CurrentApplicant
    {
        get => currentApplicant;
        private set
        {
            if (SetProperty(ref currentApplicant, value))
            {
                HasApplicant = value is not null;
                IsContactRevealed = false;
                IsExpanded = false;
                ScoreBreakdown = null;
                RaiseDerivedPropertyChanges();
            }
        }
    }

    public CompatibilityBreakdown? ScoreBreakdown
    {
        get => scoreBreakdown;
        private set => SetProperty(ref scoreBreakdown, value);
    }

    public bool HasApplicant
    {
        get => hasApplicant;
        private set
        {
            if (SetProperty(ref hasApplicant, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public bool IsExpanded
    {
        get => isExpanded;
        private set => SetProperty(ref isExpanded, value);
    }

    public bool IsContactRevealed
    {
        get => isContactRevealed;
        private set
        {
            if (SetProperty(ref isContactRevealed, value))
            {
                OnPropertyChanged(nameof(MaskedEmail));
                OnPropertyChanged(nameof(MaskedPhone));
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

    public bool IsLoading
    {
        get => isLoading;
        private set
        {
            if (SetProperty(ref isLoading, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public string MaskedEmail => CurrentApplicant is null
        ? string.Empty
        : IsContactRevealed ? CurrentApplicant.User.Email : ViewModelSupport.MaskEmail(CurrentApplicant.User.Email);

    public string MaskedPhone => CurrentApplicant is null
        ? string.Empty
        : IsContactRevealed ? CurrentApplicant.User.Phone : ViewModelSupport.MaskPhone(CurrentApplicant.User.Phone);

    public IReadOnlyList<SkillDisplay> TopSkills => CurrentApplicant is null
        ? new List<SkillDisplay>()
        : ViewModelSupport.BuildSkillDisplay(CurrentApplicant.UserSkills, 5);

    public int RemainingSkillCount => CurrentApplicant is null
        ? 0
        : Math.Max(0, CurrentApplicant.UserSkills.Count - 5);

    public IReadOnlyList<SkillDisplay> AllSkills => CurrentApplicant is null
        ? new List<SkillDisplay>()
        : ViewModelSupport.BuildSkillDisplay(CurrentApplicant.UserSkills, null);

    public ICommand AdvanceCommand => advanceCommand;
    public ICommand SkipCommand => skipCommand;
    public ICommand UndoCommand => undoCommand;
    public ICommand RefreshCommand => refreshCommand;
    public ICommand ExpandCommand => expandCommand;
    public ICommand CollapseCommand => collapseCommand;

    public async Task LoadApplicantsAsync(CancellationToken cancellationToken = default)
    {
        if (session.Mode != AppMode.Company || session.CompanyId is null)
        {
            CurrentApplicant = null;
            StatusMessage = "Company mode is not active.";
            return;
        }

        IsLoading = true;
        StatusMessage = string.Empty;

        try
        {
            await recommendationService.LoadApplicantsAsync(session.CompanyId.Value, cancellationToken);
            LoadNextApplicant();
        }
        catch (Exception exception)
        {
            CurrentApplicant = null;
            ReportError($"Could not load applicants: {exception.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task AdvanceApplicantAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentApplicant is null || !ValidateSession())
        {
            return;
        }

        if (!await ValidateApplicantStateAsync(cancellationToken))
        {
            return;
        }

        try
        {
            await matchService.AdvanceAsync(CurrentApplicant.Match.MatchId, cancellationToken);
            StoreForUndo();
            recommendationService.MoveToNext();
            LoadNextApplicant();
        }
        catch (Exception exception)
        {
            ReportError($"Could not advance applicant: {exception.Message}");
        }
    }

    public async Task SkipApplicantAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentApplicant is null || !ValidateSession())
        {
            return;
        }

        if (!await ValidateApplicantStateAsync(cancellationToken))
        {
            return;
        }

        try
        {
            await matchService.RejectAsync(CurrentApplicant.Match.MatchId, "Rejected on first pass", cancellationToken);
            StoreForUndo();
            recommendationService.MoveToNext();
            LoadNextApplicant();
        }
        catch (Exception exception)
        {
            ReportError($"Could not skip applicant: {exception.Message}");
        }
    }

    public async Task UndoLastActionAsync(CancellationToken cancellationToken = default)
    {
        if (lastUndoApplicant is null)
        {
            return;
        }

        try
        {
            await matchService.RevertToAppliedAsync(lastUndoApplicant.Match.MatchId, cancellationToken);
            recommendationService.MoveToPrevious();

            CurrentApplicant = lastUndoApplicant;
            IsContactRevealed = false;
            StatusMessage = string.Empty;
            lastUndoApplicant = null;
            undoUsed = true;
            CanUndo = false;
            RaiseDerivedPropertyChanges();
        }
        catch (Exception exception)
        {
            ReportError($"Could not undo: {exception.Message}");
        }
    }

    public async Task ExpandCardAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentApplicant is null)
        {
            return;
        }

        ScoreBreakdown = await recommendationService.GetBreakdownAsync(CurrentApplicant, cancellationToken);
        IsExpanded = true;
        RaiseDerivedPropertyChanges();
    }

    public void CollapseCard()
    {
        IsExpanded = false;
    }

    private void LoadNextApplicant()
    {
        var next = recommendationService.GetNextApplicant();
        CurrentApplicant = next;
        StatusMessage = next is null ? "No more applicants to review." : string.Empty;
        RaiseDerivedPropertyChanges();
    }

    private bool ValidateSession()
    {
        if (session.CompanyId is null)
        {
            ReportError("Company context is not available.");
            return false;
        }

        if (CurrentApplicant!.Job.Company.CompanyId != session.CompanyId.Value)
        {
            ReportError("This applicant does not belong to your company.");
            return false;
        }

        return true;
    }

    private async Task<bool> ValidateApplicantStateAsync(CancellationToken cancellationToken)
    {
        var freshMatch = await matchService.GetByIdAsync(CurrentApplicant!.Match.MatchId, cancellationToken);
        if (freshMatch is null || freshMatch.Status != MatchStatus.Applied)
        {
            ReportError("This applicant has already been reviewed. Loading next applicant.");
            recommendationService.MoveToNext();
            LoadNextApplicant();
            return false;
        }

        return true;
    }

    private void StoreForUndo()
    {
        if (!undoUsed)
        {
            lastUndoApplicant = CurrentApplicant;
            CanUndo = true;
        }
    }

    private void RaiseDerivedPropertyChanges()
    {
        OnPropertyChanged(nameof(TopSkills));
        OnPropertyChanged(nameof(RemainingSkillCount));
        OnPropertyChanged(nameof(AllSkills));
        OnPropertyChanged(nameof(MaskedEmail));
        OnPropertyChanged(nameof(MaskedPhone));
    }

    private void RaiseCommandStates()
    {
        advanceCommand.NotifyCanExecuteChanged();
        skipCommand.NotifyCanExecuteChanged();
        undoCommand.NotifyCanExecuteChanged();
        refreshCommand.NotifyCanExecuteChanged();
        expandCommand.NotifyCanExecuteChanged();
    }

    private void ReportError(string message)
    {
        StatusMessage = string.Empty;
        ErrorOccurred?.Invoke(message);
    }

    private bool CanAdvanceOrSkip() => HasApplicant && !IsLoading;
    private bool CanUndoAction() => CanUndo && !IsLoading;
    private bool CanExpandCard() => HasApplicant;
    private void ExecuteRefreshCommand() => _ = LoadApplicantsAsync();
    private void ExecuteAdvanceCommand() => _ = AdvanceApplicantAsync();
    private void ExecuteSkipCommand() => _ = SkipApplicantAsync();
    private void ExecuteUndoCommand() => _ = UndoLastActionAsync();
    private void ExecuteExpandCommand() => _ = ExpandCardAsync();
}
