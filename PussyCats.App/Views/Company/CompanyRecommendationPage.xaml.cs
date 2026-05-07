using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;

namespace PussyCats_App.Views.Company;

public sealed partial class CompanyRecommendationPage : Page
{
    private readonly CompanyRecommendationViewModel viewModel;

    public CompanyRecommendationPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<CompanyRecommendationViewModel>();
        viewModel.ErrorOccurred   += OnViewModelError;
        viewModel.PropertyChanged += OnPropertyChanged;
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
    }

    private async void OnLoaded(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.LoadApplicantsAsync();
        UpdateView();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs eventArguments)
    {
        if (eventArguments.PropertyName is nameof(CompanyRecommendationViewModel.IsLoading)
                           or nameof(CompanyRecommendationViewModel.HasApplicant)
                           or nameof(CompanyRecommendationViewModel.IsExpanded)
                           or nameof(CompanyRecommendationViewModel.CanUndo)
                           or null)
        {
            DispatcherQueue.TryEnqueue(UpdateView);
        }
    }

    private async void OnAdvanceClick(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.AdvanceApplicantAsync();
        UpdateView();
    }

    private async void OnSkipClick(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.SkipApplicantAsync();
        UpdateView();
    }

    private async void OnUndoClick(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.UndoLastActionAsync();
        UpdateView();
    }

    private async void OnExpandClick(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.ExpandCardAsync();
        UpdateView();
    }

    private void OnCollapseClick(object sender, RoutedEventArgs eventArguments)
    {
        viewModel.CollapseCard();
        UpdateView();
    }

    private async void OnRefreshClick(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.LoadApplicantsAsync();
        UpdateView();
    }

    private void UpdateView()
    {
        if (viewModel.IsLoading)       { ShowLoading(); return; }
        if (!viewModel.HasApplicant)   { ShowEmptyState(); return; }
        if (viewModel.IsExpanded)      ShowExpandedView();
        else                           ShowCardView();
    }

    private void ShowCardView()
    {
        CardViewPanel.Visibility     = Visibility.Visible;
        ExpandedViewPanel.Visibility = Visibility.Collapsed;
        EmptyStatePanel.Visibility   = Visibility.Collapsed;
        LoadingPanel.Visibility      = Visibility.Collapsed;
        ActionButtonsPanel.Visibility = Visibility.Visible;
        UpdateActionButtons(true);
        BindCardData();
    }

    private void ShowExpandedView()
    {
        CardViewPanel.Visibility     = Visibility.Collapsed;
        ExpandedViewPanel.Visibility = Visibility.Visible;
        EmptyStatePanel.Visibility   = Visibility.Collapsed;
        LoadingPanel.Visibility      = Visibility.Collapsed;
        ActionButtonsPanel.Visibility = Visibility.Collapsed;
        BindExpandedData();
    }

    private void ShowEmptyState()
    {
        CardViewPanel.Visibility     = Visibility.Collapsed;
        ExpandedViewPanel.Visibility = Visibility.Collapsed;
        EmptyStatePanel.Visibility   = Visibility.Visible;
        LoadingPanel.Visibility      = Visibility.Collapsed;
        ActionButtonsPanel.Visibility = Visibility.Visible;
        UpdateActionButtons(false);
    }

    private void ShowLoading()
    {
        CardViewPanel.Visibility     = Visibility.Collapsed;
        ExpandedViewPanel.Visibility = Visibility.Collapsed;
        EmptyStatePanel.Visibility   = Visibility.Collapsed;
        LoadingPanel.Visibility      = Visibility.Visible;
        ActionButtonsPanel.Visibility = Visibility.Collapsed;
    }

    private void UpdateActionButtons(bool enabled)
    {
        SkipButton.IsEnabled    = enabled;
        AdvanceButton.IsEnabled = enabled;
        UndoButton.IsEnabled    = viewModel.CanUndo;
    }

    private void BindCardData()
    {
        var applicant = viewModel.CurrentApplicant;
        if (applicant is null) return;

        var name = $"{applicant.User.FirstName} {applicant.User.LastName}".Trim();
        AvatarInitial.Text    = name.Length > 0 ? name[..1].ToUpperInvariant() : "?";
        ApplicantNameText.Text = name;
        JobTitleText.Text     = string.IsNullOrWhiteSpace(applicant.Job.JobTitle)
            ? applicant.Job.JobDescription
            : applicant.Job.JobTitle;
        MatchScoreText.Text   = $"{applicant.CompatibilityScore:F0}%";
        LocationText.Text     = $"{applicant.User.City}, {applicant.User.Country}".Trim(',', ' ');
        ExperienceText.Text   = $"{applicant.User.YearsOfExperience} yrs";
        EducationText.Text    = applicant.User.University;

        TopSkillsList.ItemsSource = viewModel.TopSkills;
        MoreSkillsText.Text       = viewModel.RemainingSkillCount > 0
            ? $"+{viewModel.RemainingSkillCount} more skills"
            : string.Empty;
        MoreSkillsText.Visibility = viewModel.RemainingSkillCount > 0 ? Visibility.Visible : Visibility.Collapsed;

        UndoButton.IsEnabled = viewModel.CanUndo;
    }

    private void BindExpandedData()
    {
        var applicant = viewModel.CurrentApplicant;
        if (applicant is null) return;

        var name = $"{applicant.User.FirstName} {applicant.User.LastName}".Trim();
        ExpandedNameText.Text       = name;
        ExpandedJobText.Text        = $"Applied for: {(string.IsNullOrWhiteSpace(applicant.Job.JobTitle) ? applicant.Job.JobDescription : applicant.Job.JobTitle)}";
        ExpandedMatchScoreText.Text = $"{applicant.CompatibilityScore:F0}% Match";
        ExpandedLocationText.Text   = $"{applicant.User.City}, {applicant.User.Country}".Trim(',', ' ');
        ExpandedExperienceText.Text = $"{applicant.User.YearsOfExperience} years";
        ExpandedEducationText.Text  = applicant.User.University;
        ResumeText.Text             = string.IsNullOrWhiteSpace(applicant.User.Motivation)
            ? "No motivation text provided."
            : applicant.User.Motivation;
        JobDescriptionText.Text     = applicant.Job.JobDescription;
        AllSkillsList.ItemsSource   = viewModel.AllSkills;

        var breakdown = viewModel.ScoreBreakdown;
        if (breakdown is not null)
        {
            BreakdownSkillText.Text      = $"{breakdown.SkillScore:F1}";
            BreakdownKeywordText.Text    = $"{breakdown.KeywordScore:F1}";
            BreakdownPreferenceText.Text = $"{breakdown.PreferenceScore:F1}";
            BreakdownPromotionText.Text  = $"{breakdown.PromotionScore:F1}";
        }

        ContactEmailText.Text = viewModel.MaskedEmail;
        ContactPhoneText.Text = viewModel.MaskedPhone;
    }

    private void OnViewModelError(string message)
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            var dialog = new ContentDialog
            {
                Title = "Error", Content = message, CloseButtonText = "OK", XamlRoot = XamlRoot,
            };
            await dialog.ShowAsync();
        });
    }
}
