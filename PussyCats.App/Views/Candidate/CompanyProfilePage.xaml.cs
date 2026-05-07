using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;

namespace PussyCats_App.Views.Candidate;

public sealed partial class CompanyProfilePage : Page
{
    private readonly CompanyProfileViewModel viewModel;

    public CompanyProfilePage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<CompanyProfileViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);

        if (eventArguments.Parameter is not int companyId || companyId <= 0)
        {
            ShowError("No company ID provided.");
            return;
        }

        await viewModel.LoadAsync(companyId);
        UpdateUI();
    }

    private void UpdateUI()
    {
        var company = viewModel.Company;

        if (!string.IsNullOrWhiteSpace(viewModel.ErrorMessage))
        {
            ShowError(viewModel.ErrorMessage);
            return;
        }

        if (company is null)
        {
            ShowError("Company not found.");
            return;
        }

        ErrorBorder.Visibility = Visibility.Collapsed;

        CompanyNameTextBlock.Text = company.CompanyName;
        LogoTextBlock.Text = string.IsNullOrWhiteSpace(company.LogoText)
            ? GetInitials(company.CompanyName)
            : company.LogoText;
        EmailTextBlock.Text = string.IsNullOrWhiteSpace(company.Email) ? "—" : company.Email;
        PhoneTextBlock.Text = string.IsNullOrWhiteSpace(company.Phone) ? "—" : company.Phone;

        if (company.Jobs.Count == 0)
        {
            JobsList.Visibility = Visibility.Collapsed;
            NoJobsTextBlock.Visibility = Visibility.Visible;
        }
        else
        {
            JobsList.ItemsSource = company.Jobs;
            JobsList.Visibility = Visibility.Visible;
            NoJobsTextBlock.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowError(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorBorder.Visibility = Visibility.Visible;
    }

    private void BackButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant()
            : name[..Math.Min(2, name.Length)].ToUpperInvariant();
    }
}
