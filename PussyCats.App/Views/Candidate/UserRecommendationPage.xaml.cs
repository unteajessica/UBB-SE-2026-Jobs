using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;

namespace PussyCats_App.Views.Candidate;

public sealed partial class UserRecommendationPage : Page
{
    private readonly UserRecommendationViewModel viewModel;
    private bool isErrorDialogOpen;
    private ContentDialog? errorDialog;

    public UserRecommendationPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<UserRecommendationViewModel>();
        viewModel.ErrorOccurred          += OnViewModelError;
        viewModel.PropertyChanged        += OnPropertyChanged;
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
    }

    private async void OnLoaded(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.InitializeAsync();
        UpdateView();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs eventArguments)
    {
        if (eventArguments.PropertyName is nameof(UserRecommendationViewModel.IsLoading)
                           or nameof(UserRecommendationViewModel.CurrentJob)
                           or nameof(UserRecommendationViewModel.IsDetailOpen)
                           or nameof(UserRecommendationViewModel.CanUndo)
                           or nameof(UserRecommendationViewModel.HasCard)
                           or nameof(UserRecommendationViewModel.ShowEmptyDeck)
                           or null)
        {
            DispatcherQueue.TryEnqueue(UpdateView);
        }
    }

    private async void OnApplyFiltersClick(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.ApplyFiltersAsync();
        UpdateView();
    }

    private void OnResetDraftFiltersClick(object sender, RoutedEventArgs eventArguments)
        => viewModel.ResetDraftFilters();

    private void OnOpenFiltersClick(object sender, RoutedEventArgs eventArguments)
        => viewModel.IsFilterOpen = true;

    private void OnRefreshClick(object sender, RoutedEventArgs eventArguments)
    {
        viewModel.LoadRecommendationsAsync();
        UpdateView();
    }

    private void OnExpandClick(object sender, RoutedEventArgs eventArguments)
    {
        viewModel.ExpandCard();
        UpdateView();
    }

    private void OnCollapseClick(object sender, RoutedEventArgs eventArguments)
    {
        viewModel.CollapseCard();
        UpdateView();
    }

    private async void OnLikeClick(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.LikeAsync();
        UpdateView();
    }

    private async void OnDismissClick(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.DismissAsync();
        UpdateView();
    }

    private async void OnUndoClick(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.UndoAsync();
        UpdateView();
    }

    private void UpdateView()
    {
        if (viewModel.IsLoading)       { ShowLoading(); return; }
        if (!viewModel.HasCard)        { ShowEmptyState(); return; }
        if (viewModel.IsDetailOpen)    ShowExpandedView();
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
        DismissButton.IsEnabled = enabled;
        LikeButton.IsEnabled    = enabled;
        UndoButton.IsEnabled    = viewModel.CanUndo;
    }

    private void BindCardData()
    {
        var job = viewModel.CurrentJob;
        if (job is null) return;

        var name = job.Company.CompanyName;
        CardCompanyInitial.Text   = string.IsNullOrWhiteSpace(job.Company.LogoText)
            ? (name.Length > 0 ? name[..1].ToUpperInvariant() : "?")
            : job.Company.LogoText;
        CardCompanyNameText.Text  = name;
        CardJobTitleText.Text     = job.JobTitleLine;
        CardMatchScoreText.Text   = $"{job.CompatibilityScore:F0}%";
        CardLocationEmploymentText.Text = job.LocationEmploymentLine;
        CardTopSkillsList.ItemsSource   = job.TopSkillLabels;
        CardDescriptionExcerptText.Text = job.DescriptionExcerpt;
        UndoButton.IsEnabled = viewModel.CanUndo;
    }

    private void BindExpandedData()
    {
        var job = viewModel.CurrentJob;
        if (job is null) return;

        ExpandedMatchScoreText.Text  = $"{job.CompatibilityScore:F0}% Match";
        ExpandedCompanyText.Text     = job.Company.CompanyName;
        ExpandedJobTitleText.Text    = job.JobTitleLine;
        ExpandedLocationText.Text    = job.LocationEmploymentLine;
        ExpandedJobDescriptionText.Text = job.Job.JobDescription;
        ExpandedAllSkillsList.ItemsSource = job.AllSkillLabels;
        ExpandedContactText.Text     = job.ContactLine;
    }

    private void OnViewModelError(string message)
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            if (isErrorDialogOpen)
            {
                return;
            }

            isErrorDialogOpen = true;
            errorDialog ??= new ContentDialog
            {
                Title = "Error",
                CloseButtonText = "OK",
            };

            errorDialog.Content = message;
            errorDialog.XamlRoot = XamlRoot;
            await errorDialog.ShowAsync();
            isErrorDialogOpen = false;
        });
    }
}
