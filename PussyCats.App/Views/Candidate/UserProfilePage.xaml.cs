using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Configuration;
using PussyCats.Library.Services;
using PussyCats.App.ViewModels;
using PussyCats_App.Views.Controls;

namespace PussyCats_App.Views.Candidate;

public sealed partial class UserProfilePage : Page
{
    private readonly UserProfileViewModel viewModel;
    private bool isBusy;

    public UserProfilePage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<UserProfileViewModel>();
        viewModel.LevelUpdated       += RefreshLevelDisplay;
        viewModel.PersonalityTestRequested += () => Frame.Navigate(typeof(PersonalityTestPage));
    }

    protected override async void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
        await LoadAsync();
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        isBusy = true;
        await viewModel.LoadUserAsync();
        BindLabels();
        isBusy = false;
    }

    private void BindLabels()
    {
        var user = viewModel.UserProfile;
        errorLabel.Text = viewModel.ErrorMessage;

        if (user is null) return;

        firstNameLabel.Text  = $"First Name: {user.FirstName}";
        lastNameLabel.Text   = $"Last Name: {user.LastName}";
        emailLabel.Text      = $"Email: {user.Email}";
        phoneLabel.Text      = $"Phone: {user.Phone}";
        githubLabel.Text     = $"GitHub: {user.GitHub}";
        linkedinLabel.Text   = $"LinkedIn: {user.LinkedIn}";
        genderLabel.Text     = $"Gender: {user.Gender}";
        universityLabel.Text = $"University: {user.University}";
        gradYearLabel.Text   = $"Graduation Year: {user.ExpectedGraduationYear}";
        countryLabel.Text    = $"Country: {user.Country}";
        cityLabel.Text       = $"City: {user.City}";

        var roleText = user.PersonalityResult?.SelectedRole is { } role
            ? ViewModelSupport.FormatJobRole(role)
            : "Not taken yet";
        personalityLabel.Text = $"Personality Test Result: {roleText}";

        freshnessLabel.Text = viewModel.FreshnessText;
        checkAccountStatus.IsOn = user.ActiveAccount;
        buttonPersonalityTest.Content = viewModel.GetPersonalityButtonText();
        if (!string.IsNullOrEmpty(user.ProfilePicturePath))
        {
            var fullUrl = $"https://localhost:7134/api/files/{user.ProfilePicturePath}";
            publicAvatar.ProfilePicture = new BitmapImage(new Uri(fullUrl));
        }
        else
            publicAvatar.ProfilePicture = null;
        /*

        if (!string.IsNullOrEmpty(user.ProfilePicturePath))

            publicAvatar.ProfilePicture = new BitmapImage(new Uri(user.ProfilePicturePath));
        else
            publicAvatar.ProfilePicture = null;*/

        completenessBar.Update(viewModel.CompletenessPercentage, viewModel.NextEmptyFieldPrompt);
        RefreshLevelDisplay();
    }

    private void RefreshLevelDisplay()
    {
        var user = viewModel.UserProfile;
        if (user is null) return;

        LevelTitleText.Text = $"Level {user.CurrentLevel}";
        ExperienceProgressBar.Value = UserLevelService.GetLevelProgressPercent(
            viewModel.TotalExperiencePoints, user.CurrentLevel);
        var toNext = UserLevelService.GetExperiencePointsToNextLevel(
            viewModel.TotalExperiencePoints, user.CurrentLevel);
        ExperienceCountText.Text = toNext > 0
            ? $"{viewModel.TotalExperiencePoints} XP — {toNext} XP to next level"
            : $"{viewModel.TotalExperiencePoints} XP — Max level!";
    }

    private async void OnAvatarUploadClick(object sender, RoutedEventArgs eventArguments)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        var handle = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, handle);
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;

        using var stream = await file.OpenStreamForReadAsync();
        await viewModel.UploadAvatarAsync(stream, file.Name);
        BindLabels();
    }

    private async void OnAvatarRemoveClick(object sender, RoutedEventArgs eventArguments)
    {
        await viewModel.RemoveAvatarAsync();
        BindLabels();
    }

    private async void OnStatusToggle(object sender, RoutedEventArgs eventArguments)
    {
        if (isBusy) return;
        await viewModel.ToggleAccountStatusAsync();
        BindLabels();
    }

    private void OnEditProfileClick(object sender, RoutedEventArgs eventArguments)
        => Frame.Navigate(typeof(ProfileFormPage), viewModel.UserProfile);

    private void OnPreviewCVClick(object sender, RoutedEventArgs eventArguments)
        => Frame.Navigate(typeof(ExportCVPage));

    private void OnViewDocumentsClick(object sender, RoutedEventArgs eventArguments)
        => Frame.Navigate(typeof(DocumentsPage));

    private void OnMatchHistoryClick(object sender, RoutedEventArgs eventArguments)
        => Frame.Navigate(typeof(MatchHistoryPage));

    private void OnCompatibilityClick(object sender, RoutedEventArgs eventArguments)
        => Frame.Navigate(typeof(CompatibilityOverviewPage));

    private void OnPublicProfileClick(object sender, RoutedEventArgs eventArguments)
        => Frame.Navigate(typeof(PublicProfilePage), viewModel.UserProfile?.UserId ?? 0);

    private void OnSkillTestsClick(object sender, RoutedEventArgs eventArguments)
        => Frame.Navigate(typeof(TestDashboardPage), viewModel.UserProfile);

    private void OnPersonalityTestClick(object sender, RoutedEventArgs eventArguments)
        => Frame.Navigate(typeof(PersonalityTestPage));
}
