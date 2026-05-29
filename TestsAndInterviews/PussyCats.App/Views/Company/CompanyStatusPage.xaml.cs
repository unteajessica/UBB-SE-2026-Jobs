using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain.Enums;
using PussyCats.App.ViewModels;

namespace PussyCats_App.Views.Company;

public sealed partial class CompanyStatusPage : Page
{
    private readonly CompanyStatusViewModel viewModel;
    private readonly ContentDialog dialog = new();
    private readonly SolidColorBrush defaultBorder = new(Microsoft.UI.Colors.LightGray);
    private readonly SolidColorBrush errorBorder   = new(Microsoft.UI.Colors.IndianRed);
    private bool isDialogOpen;

    public CompanyStatusPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<CompanyStatusViewModel>();
        viewModel.ErrorOccurred += OnViewModelError;
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
    }

    private async void OnLoaded(object sender, RoutedEventArgs eventArguments)
    {
        var session = App.Services.GetRequiredService<SessionContext>();
        if (session.Mode != AppMode.Company || session.CompanyId is null) return;

        await viewModel.LoadApplicationsAsync();
        ShowApplicantList();
        ResetValidationVisuals();
    }

    private async void OnReviewApplicantClick(object sender, RoutedEventArgs eventArguments)
    {
        if (sender is not Button { Tag: int matchId }) return;
        var loaded = await viewModel.LoadEvaluationAsync(matchId);
        if (loaded) { ShowEvaluation(); ResetValidationVisuals(); }
        else ShowApplicantList();
    }

    private async void OnSubmitDecisionClick(object sender, RoutedEventArgs eventArguments)
    {
        ResetValidationVisuals();
        var saved = await viewModel.SubmitDecisionAsync();

        if (viewModel.HasValidationErrors)
        {
            ApplyValidationVisuals();
            await ShowDialogAsync("Validation", "Please fix the highlighted fields.");
            return;
        }

        if (!saved)
        {
            await ShowDialogAsync("Operation Failed", "The decision was not saved. Please try again.");
            return;
        }

        await ShowDialogAsync("Success", "Decision submitted successfully.");
        ShowApplicantList();
    }

    private async void OnCancelEvaluationClick(object sender, RoutedEventArgs eventArguments)
    {
        var confirmed = await ShowConfirmationAsync("Cancel", "Are you sure you want to cancel the evaluation?");
        if (!confirmed) return;
        viewModel.CancelEvaluation();
        ShowApplicantList();
    }

    private void ShowApplicantList()
    {
        ApplicantListPanel.Visibility = Visibility.Visible;
        EvaluationPanel.Visibility   = Visibility.Collapsed;
    }

    private void ShowEvaluation()
    {
        ApplicantListPanel.Visibility = Visibility.Collapsed;
        EvaluationPanel.Visibility   = Visibility.Visible;
    }

    private void ResetValidationVisuals()
    {
        DecisionFieldBorder.BorderBrush = defaultBorder;
        FeedbackFieldBorder.BorderBrush = defaultBorder;
    }

    private void ApplyValidationVisuals()
    {
        DecisionFieldBorder.BorderBrush = string.IsNullOrWhiteSpace(viewModel.ValidationErrorDecision)
            ? defaultBorder : errorBorder;
        FeedbackFieldBorder.BorderBrush = string.IsNullOrWhiteSpace(viewModel.ValidationErrorFeedback)
            ? defaultBorder : errorBorder;
    }

    private async void OnViewModelError(string message) => await ShowDialogAsync("Error", message);

    private async Task ShowDialogAsync(string title, string content)
    {
        if (isDialogOpen)
        {
            return;
        }

        isDialogOpen = true;
        try
        {
            ConfigureDialog(title, content, primaryButtonText: string.Empty, closeButtonText: "OK");
            await dialog.ShowAsync();
        }
        finally
        {
            isDialogOpen = false;
        }
    }

    private async Task<bool> ShowConfirmationAsync(string title, string content)
    {
        if (isDialogOpen)
        {
            return false;
        }

        isDialogOpen = true;
        try
        {
            ConfigureDialog(title, content, primaryButtonText: "Yes", closeButtonText: "No");
            return await dialog.ShowAsync() == ContentDialogResult.Primary;
        }
        finally
        {
            isDialogOpen = false;
        }
    }

    private void ConfigureDialog(string title, string content, string primaryButtonText, string closeButtonText)
    {
        dialog.Title = title;
        dialog.Content = content;
        dialog.PrimaryButtonText = primaryButtonText;
        dialog.SecondaryButtonText = string.Empty;
        dialog.CloseButtonText = closeButtonText;
        dialog.DefaultButton = ContentDialogButton.Close;
        dialog.XamlRoot = XamlRoot;
    }
}
