using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Dtos.TI;
using PussyCats.App.ViewModels.TI;
using PussyCats_App;

namespace PussyCats_App.Views.TestsAndInterviews;

public sealed partial class TiJobDetailsPage : Page
{
    public TiJobDetailsViewModel ViewModel { get; }

    public TiJobDetailsPage()
    {
        ViewModel = App.Services.GetRequiredService<TiJobDetailsViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is TiJobPostingDto job)
        {
            await ViewModel.LoadAsync(job);
            ApplyButton.Visibility = ViewModel.IsCompanyMode ? Visibility.Collapsed : Visibility.Visible;
            ViewApplicantsButton.Visibility = ViewModel.IsCompanyMode ? Visibility.Visible : Visibility.Collapsed;

            if (!ViewModel.IsCompanyMode)
            {
                await ViewModel.RefreshHasAppliedAsync();
                UpdateApplyButton();
            }
        }
    }

    private void UpdateApplyButton()
    {
        ApplyButton.Content = ViewModel.HasApplied ? "Already Applied" : "Apply Now";
        ApplyButton.IsEnabled = !ViewModel.HasApplied;
    }

    private void Back_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

    private void ViewApplicants_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CurrentJob is { } job)
            Frame.Navigate(typeof(TiJobApplicantsPage), job);
    }

    private async void Apply_Click(object sender, RoutedEventArgs e)
    {
        var job = ViewModel.CurrentJob;
        if (job is null)
            return;

        var summary = new StackPanel { Spacing = 4 };
        summary.Children.Add(new TextBlock { Text = $"Position: {job.JobTitle}" });
        summary.Children.Add(new TextBlock { Text = $"Location: {FormatText(job.JobLocation)}" });
        summary.Children.Add(new TextBlock { Text = $"Type: {FormatText(job.JobType)}" });
        if (job.Salary.HasValue)
            summary.Children.Add(new TextBlock { Text = $"Salary: {FormatSalary(job.Salary)}" });
        summary.Children.Add(new TextBlock { Text = $"Deadline: {FormatDate(job.Deadline)}" });
        summary.Children.Add(new TextBlock
        {
            Text = "By submitting this application, you confirm that the information you provide is accurate and complete.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0),
        });

        var confirmDialog = new ContentDialog
        {
            Title = $"Apply for: {job.JobTitle}",
            Content = summary,
            PrimaryButtonText = "Submit Application",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
        };

        if (await confirmDialog.ShowAsync() != ContentDialogResult.Primary)
            return;

        var (_, message) = await ViewModel.ApplyAsync();
        UpdateApplyButton();

        var resultDialog = new ContentDialog
        {
            Title = "Application",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
        };
        await resultDialog.ShowAsync();
    }

    public string FormatText(string? value) => string.IsNullOrWhiteSpace(value) ? "—" : value;

    public string FormatInt(int value) => value.ToString();

    public string FormatDate(DateTime? value) => value?.ToString("dd MMM yyyy") ?? "—";

    public string FormatSalary(int? value) => value.HasValue ? $"{value.Value:N0} RON" : "—";
}
