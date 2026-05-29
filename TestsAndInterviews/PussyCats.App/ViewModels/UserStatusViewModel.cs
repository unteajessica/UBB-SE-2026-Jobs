using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.SkillGapService;
using PussyCats.Library.Services.UserStatusService;

namespace PussyCats.App.ViewModels;

public class UserStatusViewModel : DispatchableObservableObject
{
    private readonly IUserStatusService userStatusService;
    private readonly ISkillGapService skillGapService;
    private readonly SessionContext session;
    private bool isLoading;
    private bool hasError;
    private bool isEmpty;
    private bool showCards;
    private bool hasSkillGapMessage;
    private bool showSkillData;
    private string emptyMessage = string.Empty;
    private string currentFilter = "All";
    private string skillGapMessage = string.Empty;
    private string skillGapSummaryText = string.Empty;
    private bool showGoToRecommendations;

    public UserStatusViewModel(
        IUserStatusService userStatusService,
        ISkillGapService skillGapService,
        SessionContext session)
    {
        this.userStatusService = userStatusService;
        this.skillGapService = skillGapService;
        this.session = session;

        RefreshCommand = new RelayCommand(Refresh);
        UnderscoredSkills.CollectionChanged += OnSidebarCollectionChanged;
        SkillGapMissingSkills.CollectionChanged += OnSidebarCollectionChanged;
    }

    public ObservableCollection<ApplicationCardModel> AppliedJobs { get; } = new();
    public ObservableCollection<ApplicationCardModel> FilteredJobs { get; } = new();
    public ObservableCollection<UnderscoredSkillModel> UnderscoredSkills { get; } = new();
    public ObservableCollection<MissingSkillModel> SkillGapMissingSkills { get; } = new();

    public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }
    public bool HasError { get => hasError; set => SetProperty(ref hasError, value); }
    public bool IsEmpty { get => isEmpty; set => SetProperty(ref isEmpty, value); }
    public bool ShowCards { get => showCards; set => SetProperty(ref showCards, value); }
    public bool HasSkillGapMessage { get => hasSkillGapMessage; set => SetProperty(ref hasSkillGapMessage, value); }
    public bool ShowSkillData { get => showSkillData; set => SetProperty(ref showSkillData, value); }
    public bool ShowGoToRecommendations { get => showGoToRecommendations; set => SetProperty(ref showGoToRecommendations, value); }
    public string EmptyMessage { get => emptyMessage; set => SetProperty(ref emptyMessage, value); }
    public string CurrentFilter { get => currentFilter; set => SetProperty(ref currentFilter, value); }
    public string SkillGapMessage { get => skillGapMessage; set => SetProperty(ref skillGapMessage, value); }
    public string SkillGapSummaryText { get => skillGapSummaryText; set => SetProperty(ref skillGapSummaryText, value); }
    public bool HasUnderscoredSkills => UnderscoredSkills.Count > 0;
    public bool HasSidebarMissingSkills => SkillGapMissingSkills.Count > 0;
    public ICommand RefreshCommand { get; }

    public async Task LoadMatchesAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        HasError = false;
        IsEmpty = false;
        ShowCards = false;
        HasSkillGapMessage = false;
        ShowSkillData = false;

        var userId = ViewModelSupport.ResolveUserId(session);

        try
        {
            var applications = await userStatusService.GetApplicationsForUserAsync(userId, cancellationToken);

            await UIDispatcher.EnqueueAsync(() =>
            {
                AppliedJobs.Clear();
                foreach (var application in applications)
                {
                    AppliedJobs.Add(application);
                }

                ApplyFilter(CurrentFilter);
            });
        }
        catch
        {
            HasError = true;
            ShowCards = false;
            IsLoading = false;
            return;
        }

        try
        {
            var summary = await skillGapService.GetSummaryAsync(userId, cancellationToken);
            var missingSkills = await skillGapService.GetMissingSkillsAsync(userId, cancellationToken);
            var underscoredSkills = await skillGapService.GetUnderscoredSkillsAsync(userId, cancellationToken);

            await UIDispatcher.EnqueueAsync(() =>
            {
                UnderscoredSkills.Clear();
                SkillGapMissingSkills.Clear();

                if (!summary.HasRejections)
                {
                    SkillGapMessage = "No rejections yet - keep applying to see your skill insights.";
                    HasSkillGapMessage = true;
                    ShowSkillData = false;
                }
                else if (!summary.HasSkillGaps)
                {
                    SkillGapMessage = "Great news - your skills meet the requirements of all jobs you have applied to.";
                    HasSkillGapMessage = true;
                    ShowSkillData = false;
                }
                else
                {
                    SkillGapSummaryText = $"{summary.MissingSkillsCount} missing skills - {summary.SkillsToImproveCount} skills to improve";
                    HasSkillGapMessage = false;
                    ShowSkillData = true;

                    foreach (var skill in underscoredSkills)
                    {
                        UnderscoredSkills.Add(skill);
                    }

                    foreach (var skill in missingSkills)
                    {
                        SkillGapMissingSkills.Add(skill);
                    }
                }
            });
        }
        catch
        {
            await UIDispatcher.EnqueueAsync(() =>
            {
                UnderscoredSkills.Clear();
                SkillGapMissingSkills.Clear();
                SkillGapMessage = "Skill gap analysis is temporarily unavailable.";
                HasSkillGapMessage = true;
                ShowSkillData = false;
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Refresh()
    {
        UIDispatcher.Enqueue(() =>
        {
            AppliedJobs.Clear();
            FilteredJobs.Clear();
            UnderscoredSkills.Clear();
            SkillGapMissingSkills.Clear();
            HasSkillGapMessage = false;
            ShowSkillData = false;
        });
        _ = LoadMatchesAsync();
    }

    public void ApplyFilter(string filter)
    {
        CurrentFilter = filter;
        FilteredJobs.Clear();

        foreach (var application in GetFilteredApplications(filter))
        {
            FilteredJobs.Add(application);
        }

        if (FilteredJobs.Count == 0)
        {
            IsEmpty = true;
            ShowCards = false;
            EmptyMessage = AppliedJobs.Count == 0
                ? "You have not applied to any jobs yet. Head to the Recommendations page to get started."
                : "No applications match this filter.";
            ShowGoToRecommendations = AppliedJobs.Count == 0;
        }
        else
        {
            IsEmpty = false;
            ShowCards = true;
            ShowGoToRecommendations = false;
        }
    }

    private void OnSidebarCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        OnPropertyChanged(nameof(HasUnderscoredSkills));
        OnPropertyChanged(nameof(HasSidebarMissingSkills));
    }

    private IEnumerable<ApplicationCardModel> GetFilteredApplications(string filter)
    {
        return filter switch
        {
            "Applied" => GetApplicationsByStatus(MatchStatus.Applied),
            "Accepted" => GetApplicationsByStatus(MatchStatus.Accepted),
            "Rejected" => GetApplicationsByStatus(MatchStatus.Rejected),
            _ => AppliedJobs,
        };
    }

    private IEnumerable<ApplicationCardModel> GetApplicationsByStatus(MatchStatus status)
    {
        return AppliedJobs.Where(application => application.Status == status).ToList();
    }
}
