using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace PussyCats_App.Views.Candidate;

public sealed partial class ProfileFormPage : Page
{
    private readonly ProfileFormViewModel viewModel;

    public ProfileFormPage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<ProfileFormViewModel>();
        PopulateGraduationYears();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);

        var profile = eventArguments.Parameter as User;

        if (profile != null)
        {
            viewModel.LoadProfile(profile);
        }
        else
        {
            await viewModel.LoadCurrentUserAsync();
        }

        LoadViewFromViewModel();
    }

    private void PopulateGraduationYears()
    {
        foreach (var year in viewModel.GraduationYears)
            GraduationYearComboBox.Items.Add(year.ToString());
    }

    private void LoadViewFromViewModel()
    {
        FirstNameTextBox.Text   = viewModel.FirstName;
        LastNameTextBox.Text    = viewModel.LastName;
        AgeNumberBox.Value      = viewModel.Age;
        EmailTextBox.Text       = viewModel.Email;
        GitHubTextBox.Text      = viewModel.GitHub;
        LinkedInTextBox.Text    = viewModel.LinkedIn;
        UniversityAutoSuggest.Text = viewModel.University;
        AddressTextBox.Text     = viewModel.Address;
        MotivationTextBox.Text  = viewModel.Motivation;
        PhoneTextBox.Text       = viewModel.PhoneNumber;
        CountryTextBox.Text     = viewModel.Country;
        CityTextBox.Text        = viewModel.City;
        DisabilitiesCheckBox.IsChecked = viewModel.HasDisabilities;

        SelectComboBoxItem(GenderComboBox, viewModel.Gender);
        SelectGraduationYear(viewModel.ExpectedGraduationYear);

        SkillsItemsRepeater.ItemsSource         = viewModel.Skills;
        WorkExperienceItemsRepeater.ItemsSource  = viewModel.WorkExperiences;
        ProjectsItemsRepeater.ItemsSource        = viewModel.Projects;
        ActivitiesItemsRepeater.ItemsSource      = viewModel.ExtraCurricularActivities;
    }

    private void SyncViewToViewModel()
    {
        viewModel.FirstName   = FirstNameTextBox.Text;
        viewModel.LastName    = LastNameTextBox.Text;
        viewModel.Age         = AgeNumberBox.Value;
        viewModel.Gender      = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
        viewModel.Email       = EmailTextBox.Text;
        viewModel.PhoneNumber = PhoneTextBox.Text;
        viewModel.GitHub      = GitHubTextBox.Text;
        viewModel.LinkedIn    = LinkedInTextBox.Text;
        viewModel.Country     = CountryTextBox.Text;
        viewModel.City        = CityTextBox.Text;
        viewModel.University  = UniversityAutoSuggest.Text;
        viewModel.Address     = AddressTextBox.Text;
        viewModel.Motivation  = MotivationTextBox.Text;
        viewModel.ExpectedGraduationYear = int.TryParse(
            GraduationYearComboBox.SelectedItem?.ToString(), out var yr) ? yr : 0;
    }

    private static void SelectComboBoxItem(ComboBox comboBox, string value)
    {
        foreach (ComboBoxItem item in comboBox.Items)
        {
            if (item.Content?.ToString()?.Equals(value, StringComparison.OrdinalIgnoreCase) == true)
            {
                comboBox.SelectedItem = item;
                return;
            }
        }
    }

    private void SelectGraduationYear(int year)
    {
        foreach (var item in GraduationYearComboBox.Items)
        {
            if (item.ToString() == year.ToString())
            {
                GraduationYearComboBox.SelectedItem = item;
                return;
            }
        }
    }

    private async void UploadCVButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        var picker = new FileOpenPicker();
        picker.ViewMode = PickerViewMode.List;
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".json");
        picker.FileTypeFilter.Add(".xml");

        var handle = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, handle);

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;

        var content = await FileIO.ReadTextAsync(file);
        viewModel.ProcessCvFile(content, file.FileType);
        LoadViewFromViewModel();
        CVStatusText.Text = viewModel.CvStatusText;
        CVUploadInformationBar.Message  = viewModel.InfoBarMessage;
        CVUploadInformationBar.Severity = (InfoBarSeverity)viewModel.InfoBarSeverity;
        CVUploadInformationBar.IsOpen   = viewModel.IsInfoBarOpen;
    }

    private void UniversityAutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs eventArguments)
    {
        if (eventArguments.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            sender.ItemsSource = viewModel.FilterUniversities(sender.Text);
    }

    private void UniversityAutoSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs eventArguments)
    {
        if (eventArguments.ChosenSuggestion is not null)
            sender.Text = eventArguments.ChosenSuggestion.ToString()!;
    }

    private void SkillsAutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs eventArguments)
    {
        if (eventArguments.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            sender.ItemsSource = viewModel.FilterSkillSuggestions(sender.Text);
    }

    private void SkillsAutoSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs eventArguments)
    {
        viewModel.AddSkill(eventArguments.ChosenSuggestion?.ToString() ?? sender.Text);
        sender.Text = string.Empty;
    }

    private void AddSkillButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        viewModel.AddSkill(SkillsAutoSuggest.Text);
        SkillsAutoSuggest.Text = string.Empty;
    }

    private void RemoveSkillButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (sender is Button { Tag: string skill })
            viewModel.RemoveSkill(skill);
    }

    private void AddWorkExperienceButton_Click(object sender, RoutedEventArgs eventArguments)
        => viewModel.AddWorkExperience();

    private void RemoveWorkExperienceButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (sender is Button { Tag: WorkExperience we })
            viewModel.RemoveWorkExperience(we);
    }

    private void AddProjectButton_Click(object sender, RoutedEventArgs eventArguments)
        => viewModel.AddProject();

    private void RemoveProjectButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (sender is Button { Tag: Project proj })
            viewModel.RemoveProject(proj);
    }

    private void AddActivityButton_Click(object sender, RoutedEventArgs eventArguments)
        => viewModel.AddExtraCurricularActivity();

    private void RemoveActivityButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (sender is Button { Tag: ExtraCurricularActivity act })
            viewModel.RemoveExtraCurricularActivity(act);
    }

    private void NameTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs eventArguments)
        => eventArguments.Cancel = eventArguments.NewText.Any(char.IsDigit);

    private void PhoneNumberTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs eventArguments)
        => eventArguments.Cancel = eventArguments.NewText.Any(c => !char.IsDigit(c));

    private async void SaveButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        SyncViewToViewModel();
        var success = await viewModel.SaveProfileAsync();
        FormValidationInformationBar.Message  = viewModel.InfoBarMessage;
        FormValidationInformationBar.Severity = (InfoBarSeverity)viewModel.InfoBarSeverity;
        FormValidationInformationBar.IsOpen   = !success;

        if (success)
        {
            if (Frame.CanGoBack) Frame.GoBack();
            else Frame.Navigate(typeof(UserProfilePage));
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (Frame.CanGoBack) Frame.GoBack();
        else Frame.Navigate(typeof(UserProfilePage));
    }

    private void EditPreferencesButton_Click(object sender, RoutedEventArgs eventArguments)
    {
        SyncViewToViewModel();
        Frame.Navigate(typeof(PreferencesPage));
    }
}
