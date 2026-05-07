using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain.Enums;

namespace PussyCats_App.Views.Candidate;

public sealed partial class PreferencesPage : Page
{
    private readonly PreferencesViewModel viewModel;

    private static readonly System.Collections.Generic.Dictionary<JobRole, string> RoleDisplayNames = new()
    {
        { JobRole.FrontendDeveloper,      "Frontend Developer" },
        { JobRole.BackendDeveloper,        "Backend Developer" },
        { JobRole.UiUxDesigner,            "UI/UX Designer" },
        { JobRole.DevOpsEngineer,          "DevOps Engineer" },
        { JobRole.ProjectManager,          "Project Manager" },
        { JobRole.DataAnalyst,             "Data Analyst" },
        { JobRole.CybersecuritySpecialist, "Cybersecurity Specialist" },
        { JobRole.AiMlEngineer,            "AI/ML Engineer" },
    };

    public PreferencesPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<PreferencesViewModel>();
        RolesListView.ItemsSource = RoleDisplayNames.Values.ToList();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
        await viewModel.LoadPreferencesAsync();
        PopulateFromViewModel();
    }

    private void PopulateFromViewModel()
    {
        RolesListView.SelectionChanged -= RolesListView_SelectionChanged;
        RolesListView.SelectedItems.Clear();
        foreach (var role in viewModel.GetSelectedJobRoles())
        {
            if (RoleDisplayNames.TryGetValue(role, out var name))
            {
                var idx = RolesListView.Items.Cast<string>().ToList().IndexOf(name);
                if (idx >= 0) RolesListView.SelectedItems.Add(RolesListView.Items[idx]);
            }
        }
        RolesListView.SelectionChanged += RolesListView_SelectionChanged;

        var workMode = viewModel.GetSelectedWorkMode().ToString();
        foreach (var item in WorkModeComboBox.Items)
        {
            if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == workMode)
            {
                WorkModeComboBox.SelectedItem = cbi;
                break;
            }
        }

        LocationAutoSuggestBox.Text = viewModel.GetPreferredLocation();
    }

    private void RolesListView_SelectionChanged(object sender, SelectionChangedEventArgs eventArguments)
    {
        foreach (var item in eventArguments.AddedItems.Cast<string>())
        {
            var role = RoleDisplayNames.First(keyValuePair => keyValuePair.Value == item).Key;
            viewModel.ToggleJobRole(role);
        }
        foreach (var item in eventArguments.RemovedItems.Cast<string>())
        {
            var role = RoleDisplayNames.First(keyValuePair => keyValuePair.Value == item).Key;
            viewModel.ToggleJobRole(role);
        }

        var error = viewModel.GetErrorMessage();
        RoleWarningText.Visibility = string.IsNullOrEmpty(error) ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void LocationAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs eventArguments)
    {
        if (eventArguments.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            await viewModel.SearchLocationAsync(sender.Text);
            sender.ItemsSource = viewModel.GetLocationSuggestions();
        }
    }

    private void LocationAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs eventArguments)
    {
        var chosen = eventArguments.SelectedItem.ToString()!;
        sender.Text = chosen;
        viewModel.SetLocation(chosen);
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        var workMode = (WorkModeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        if (!string.IsNullOrEmpty(workMode) && Enum.TryParse<WorkMode>(workMode, out var parsed))
            viewModel.SetWorkMode(parsed);

        viewModel.SetLocation(LocationAutoSuggestBox.Text);
        await viewModel.SavePreferencesAsync();

        var error = viewModel.GetErrorMessage();
        SuccessMessage.Text = string.IsNullOrEmpty(error) ? "Preferences saved!" : error;
        SuccessMessage.Foreground = new SolidColorBrush(
            string.IsNullOrEmpty(error) ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
        SuccessMessage.Visibility = Visibility.Visible;
    }
}
