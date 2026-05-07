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
    private readonly SolidColorBrush defaultBorder = new(Microsoft.UI.Colors.LightGray);
    private readonly SolidColorBrush errorBorder   = new(Microsoft.UI.Colors.IndianRed);

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

    private Task ShowDialogAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title, Content = content, CloseButtonText = "OK", XamlRoot = XamlRoot,
        };
        return dialog.ShowAsync().AsTask();
    }

    private async Task<bool> ShowConfirmationAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title, Content = content,
            PrimaryButtonText = "Yes", CloseButtonText = "No",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }
}
