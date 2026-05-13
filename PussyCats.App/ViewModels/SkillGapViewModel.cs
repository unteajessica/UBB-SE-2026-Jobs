using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.Library.DTOs;
using PussyCats_App.Services.SkillGapService;

namespace PussyCats.App.ViewModels;

public class SkillGapViewModel : DispatchableObservableObject
{
    private readonly ISkillGapService skillGapService;
    private readonly SessionContext session;
    private bool isLoading;
    private bool showContent;
    private bool hasSkillData;
    private bool hasSummaryMessage;
    private string summaryMessage = string.Empty;
    private int missingCount;
    private int improveCount;

    public SkillGapViewModel(ISkillGapService skillGapService, SessionContext session)
    {
        this.skillGapService = skillGapService;
        this.session = session;
        RefreshCommand = new RelayCommand(Refresh);
        SkillsToImprove.CollectionChanged += OnCollectionChanged;
        MissingSkills.CollectionChanged += OnCollectionChanged;
    }

    public ObservableCollection<UnderscoredSkillModel> SkillsToImprove { get; } = new();
    public ObservableCollection<MissingSkillModel> MissingSkills { get; } = new();

    public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }
    public bool ShowContent { get => showContent; set => SetProperty(ref showContent, value); }
    public bool HasSkillData { get => hasSkillData; set => SetProperty(ref hasSkillData, value); }
    public bool HasSummaryMessage { get => hasSummaryMessage; set => SetProperty(ref hasSummaryMessage, value); }
    public string SummaryMessage { get => summaryMessage; set => SetProperty(ref summaryMessage, value); }
    public int MissingCount { get => missingCount; set => SetProperty(ref missingCount, value); }
    public int ImproveCount { get => improveCount; set => SetProperty(ref improveCount, value); }
    public bool HasSkillsToImprove => SkillsToImprove.Count > 0;
    public bool HasMissingSkills => MissingSkills.Count > 0;
    public ICommand RefreshCommand { get; }

    public async Task LoadDataAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ShowContent = false;
        HasSkillData = false;
        HasSummaryMessage = false;

        try
        {
            var userId = ViewModelSupport.ResolveUserId(session);
            var summary = await skillGapService.GetSummaryAsync(userId, cancellationToken);
            var missing = await skillGapService.GetMissingSkillsAsync(userId, cancellationToken);
            var underscored = await skillGapService.GetUnderscoredSkillsAsync(userId, cancellationToken);

            SkillsToImprove.Clear();
            MissingSkills.Clear();

            if (!summary.HasRejections)
            {
                SummaryMessage = "No rejections yet - keep applying to see your skill insights.";
                HasSummaryMessage = true;
                HasSkillData = false;
            }
            else if (!summary.HasSkillGaps)
            {
                SummaryMessage = "Great news - your skills meet the requirements of all jobs you have applied to.";
                HasSummaryMessage = true;
                HasSkillData = false;
            }
            else
            {
                MissingCount = summary.MissingSkillsCount;
                ImproveCount = summary.SkillsToImproveCount;
                HasSummaryMessage = false;
                HasSkillData = true;

                foreach (var skill in underscored)
                {
                    SkillsToImprove.Add(skill);
                }

                foreach (var skill in missing)
                {
                    MissingSkills.Add(skill);
                }
            }
        }
        catch
        {
            SummaryMessage = "Unable to load skill gap data. Please try again.";
            HasSummaryMessage = true;
            HasSkillData = false;
        }
        finally
        {
            IsLoading = false;
            ShowContent = true;
        }
    }

    public void Refresh()
    {
        SkillsToImprove.Clear();
        MissingSkills.Clear();
        HasSkillData = false;
        HasSummaryMessage = false;
        ShowContent = false;
        _ = LoadDataAsync();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        OnPropertyChanged(nameof(HasSkillsToImprove));
        OnPropertyChanged(nameof(HasMissingSkills));
    }
}
