using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.App.ViewModels;

public class CompanyStatusViewModel : DispatchableObservableObject
{
    private const int MaximumFeedbackLength = 500;

    private readonly ICompanyStatusService companyStatusService;
    private readonly IMatchService matchService;
    private readonly SessionContext session;
    private readonly RelayCommand refreshCommand;

    private UserApplicationResult? selectedApplicant;
    private Match? selectedMatch;
    private MatchStatus? selectedDecision;
    private string feedbackMessage = string.Empty;
    private bool isLoading;
    private string validationErrorDecision = string.Empty;
    private string validationErrorFeedback = string.Empty;
    private bool hasValidationErrors;
    private TestResult? lastTestResult;
    private string pageMessage = string.Empty;

    public CompanyStatusViewModel(
        ICompanyStatusService companyStatusService,
        IMatchService matchService,
        SessionContext session)
    {
        this.companyStatusService = companyStatusService;
        this.matchService = matchService;
        this.session = session;
        refreshCommand = new RelayCommand(ExecuteRefreshCommand, CanExecuteRefreshCommand);
    }

    public event Action<string>? ErrorOccurred;

    public ObservableCollection<UserApplicationResult> Applications { get; } = new();
    public ObservableCollection<MatchStatus> DecisionOptions { get; } = new()
    {
        MatchStatus.Accepted,
        MatchStatus.Rejected,
    };

    public UserApplicationResult? SelectedApplicant
    {
        get => selectedApplicant;
        set
        {
            if (SetProperty(ref selectedApplicant, value))
            {
                if (value is null)
                {
                    SelectedMatch = null;
                    SelectedDecision = null;
                    FeedbackMessage = string.Empty;
                    LastTestResult = null;
                }

                RaiseContactVisibilityProperties();
                RaiseCommandStates();
            }
        }
    }

    public Match? SelectedMatch
    {
        get => selectedMatch;
        private set
        {
            if (SetProperty(ref selectedMatch, value))
            {
                OnPropertyChanged(nameof(CanEditDecision));
                RaiseContactVisibilityProperties();
            }
        }
    }

    public string ContactEmailDisplay => SelectedApplicant is null
        ? string.Empty
        : CanRevealContact ? SelectedApplicant.User.Email : ViewModelSupport.MaskEmail(SelectedApplicant.User.Email);

    public string ContactPhoneDisplay => SelectedApplicant is null
        ? string.Empty
        : CanRevealContact ? SelectedApplicant.User.Phone : ViewModelSupport.MaskPhone(SelectedApplicant.User.Phone);

    public MatchStatus? SelectedDecision
    {
        get => selectedDecision;
        set
        {
            if (SetProperty(ref selectedDecision, value))
            {
                ValidateDecision();
                RaiseCommandStates();
            }
        }
    }

    public string FeedbackMessage
    {
        get => feedbackMessage;
        set
        {
            if (SetProperty(ref feedbackMessage, value))
            {
                ValidateFeedback();
                RaiseCommandStates();
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

    public string ValidationErrorDecision
    {
        get => validationErrorDecision;
        private set => SetProperty(ref validationErrorDecision, value);
    }

    public string ValidationErrorFeedback
    {
        get => validationErrorFeedback;
        private set => SetProperty(ref validationErrorFeedback, value);
    }

    public bool HasValidationErrors
    {
        get => hasValidationErrors;
        private set => SetProperty(ref hasValidationErrors, value);
    }

    public TestResult? LastTestResult
    {
        get => lastTestResult;
        private set => SetProperty(ref lastTestResult, value);
    }

    public string PageMessage
    {
        get => pageMessage;
        private set => SetProperty(ref pageMessage, value);
    }

    public bool CanEditDecision => SelectedMatch?.Status is MatchStatus.Applied or MatchStatus.Advanced;

    public ICommand RefreshCommand => refreshCommand;

    private bool CanRevealContact => SelectedMatch?.Status == MatchStatus.Accepted;

    public async Task LoadApplicationsAsync(CancellationToken cancellationToken = default)
    {
        if (session.Mode != AppMode.Company || session.CompanyId is null)
        {
            Applications.Clear();
            CancelEvaluation();
            ReportError("Company mode is not active.");
            return;
        }

        IsLoading = true;
        PageMessage = string.Empty;

        try
        {
            var results = await companyStatusService
                .GetApplicantsForCompanyAsync(session.CompanyId.Value, cancellationToken)
                ;

            Applications.Clear();
            foreach (var result in results)
            {
                Applications.Add(result);
            }

            CancelEvaluation();
            PageMessage = Applications.Count == 0
                ? "No applicants found with status Accepted, Rejected, or In Review."
                : $"{Applications.Count} applicant(s) are Accepted, Rejected, or In Review.";
        }
        catch (Exception exception)
        {
            Applications.Clear();
            CancelEvaluation();
            ReportError($"Could not load applicants: {exception.Message}");
        }
        finally
        {
            IsLoading = false;
        }

        RaiseCommandStates();
    }

    public async Task<bool> LoadEvaluationAsync(int matchId, CancellationToken cancellationToken = default)
    {
        if (session.CompanyId is null)
        {
            ReportError("Company context is not available.");
            return false;
        }

        try
        {
            var result = await companyStatusService
                .GetApplicantByMatchIdAsync(session.CompanyId.Value, matchId, cancellationToken)
                ;

            if (result is null)
            {
                ReportError("Selected applicant could not be loaded.");
                return false;
            }

            SelectedApplicant = result;
            SelectedMatch = result.Match;
            SelectedDecision = result.Match.Status is MatchStatus.Applied or MatchStatus.Advanced
                ? null
                : result.Match.Status;
            FeedbackMessage = result.Match.FeedbackMessage;

            ValidateAll();
            LastTestResult = LoadLatestTestResult(result);
            PageMessage = string.Empty;
            RaiseCommandStates();
            return true;
        }
        catch (Exception exception)
        {
            ReportError($"Could not load applicant details: {exception.Message}");
            return false;
        }
    }

    public bool ValidateDecision()
    {
        if (SelectedMatch is null)
        {
            ValidationErrorDecision = "Select an applicant first.";
            return false;
        }

        if (SelectedDecision is null || SelectedDecision == MatchStatus.Applied || SelectedDecision == MatchStatus.Advanced)
        {
            ValidationErrorDecision = "Select a valid decision (Accepted or Rejected).";
            return false;
        }

        ValidationErrorDecision = string.Empty;
        return true;
    }

    public bool ValidateFeedback()
    {
        if (string.IsNullOrWhiteSpace(FeedbackMessage))
        {
            ValidationErrorFeedback = "Feedback is required.";
            return false;
        }

        if (FeedbackMessage.Trim().Length > MaximumFeedbackLength)
        {
            ValidationErrorFeedback = $"Feedback must be {MaximumFeedbackLength} characters or fewer.";
            return false;
        }

        ValidationErrorFeedback = string.Empty;
        return true;
    }

    public bool ValidateAll()
    {
        var decisionValid = ValidateDecision();
        var feedbackValid = ValidateFeedback();
        HasValidationErrors = !(decisionValid && feedbackValid);
        return !HasValidationErrors;
    }

    public async Task<bool> SubmitDecisionAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedMatch is null || SelectedDecision is null)
        {
            ValidateAll();
            return false;
        }

        if (!ValidateAll())
        {
            return false;
        }

        try
        {
            await matchService
                .SubmitDecisionAsync(SelectedMatch.MatchId, SelectedDecision.Value, FeedbackMessage.Trim(), cancellationToken)
                ;
            PageMessage = "Decision saved successfully.";
            await LoadApplicationsAsync(cancellationToken);
            return true;
        }
        catch (Exception exception)
        {
            PageMessage = string.Empty;
            ReportError($"Could not save decision: {exception.Message}");
            return false;
        }
    }

    public void CancelEvaluation()
    {
        SetProperty(ref selectedApplicant, null, nameof(SelectedApplicant));
        SelectedMatch = null;
        SelectedDecision = null;
        FeedbackMessage = string.Empty;
        LastTestResult = null;
        ClearValidationErrors();
        RaiseCommandStates();
        RaiseContactVisibilityProperties();
    }

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        return LoadApplicationsAsync(cancellationToken);
    }

    private TestResult? LoadLatestTestResult(UserApplicationResult applicant)
    {
        // mock: see MergePlan section 8
        return new TestResult
        {
            MatchId = applicant.Match.MatchId,
            UserId = applicant.User.UserId,
            JobId = applicant.Job.JobId,
            ExternalUserId = applicant.User.UserId,
            PositionId = applicant.Job.JobId,
            Decision = applicant.Match.Status,
            FeedbackMessage = applicant.Match.FeedbackMessage,
            IsValid = false,
            ValidationErrors = ["Testing module is currently unavailable."],
        };
    }

    private void ClearValidationErrors()
    {
        ValidationErrorDecision = string.Empty;
        ValidationErrorFeedback = string.Empty;
        HasValidationErrors = false;
    }

    private void RaiseCommandStates()
    {
        refreshCommand.NotifyCanExecuteChanged();
    }

    private void RaiseContactVisibilityProperties()
    {
        OnPropertyChanged(nameof(ContactEmailDisplay));
        OnPropertyChanged(nameof(ContactPhoneDisplay));
    }

    private void ReportError(string message)
    {
        PageMessage = string.Empty;
        ErrorOccurred?.Invoke(message);
    }

    private bool CanExecuteRefreshCommand() => !IsLoading;
    private void ExecuteRefreshCommand() => _ = RefreshAsync();
}
