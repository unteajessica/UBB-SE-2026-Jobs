using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;

namespace PussyCats_App.Views.Candidate;

public sealed partial class PublicProfilePage : Page
{
    private readonly PublicProfileViewModel viewModel;

    public PublicProfilePage()
    {
        InitializeComponent();
        viewModel = App.Services.GetRequiredService<PublicProfileViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
        var userId = eventArguments.Parameter is int id ? id : eventArguments.Parameter is User user ? user.UserId : 0;
        if (userId == 0) return;

        await viewModel.LoadPublicProfileAsync(userId);

        if (!viewModel.IsAvailable)
        {
            ProfileContentPanel.Visibility      = Visibility.Collapsed;
            ProfileUnavailableTextBlock.Visibility = Visibility.Visible;
            return;
        }

        ProfileContentPanel.Visibility      = Visibility.Visible;
        ProfileUnavailableTextBlock.Visibility = Visibility.Collapsed;
        UpdateUI();
    }

    private void UpdateUI()
    {
        var profile = viewModel.Profile;
        if (profile is null) return;

        FirstNameLabel.Text  = profile.FirstName;
        LastNameLabel.Text   = profile.LastName;
        LevelLabel.Text      = $"Level {profile.CurrentLevel}";
        EmailLabel.Text      = profile.Email;
        PhoneLabel.Text      = profile.Phone;
        GenderLabel.Text     = profile.Gender;
        UniversityLabel.Text = profile.University;
        GradYearLabel.Text   = profile.ExpectedGraduationYear.ToString();
        CountryLabel.Text    = profile.Country;

        GithubLink.NavigateUri  = GetUri(profile.GitHub,  "https://github.com");
        LinkedinLink.NavigateUri = GetUri(profile.LinkedIn, "https://linkedin.com");

        if (!string.IsNullOrEmpty(profile.ProfilePicturePath))
        {
            var baseUrl = ApiConfigurationLoader.Load().BaseUrl.TrimEnd('/');
            ProfilePhoto.Source = new BitmapImage(new Uri($"{baseUrl}/api/files/{profile.ProfilePicturePath}"));
        }

        SkillTestsContainer.Children.Clear();
        foreach (var test in viewModel.Tests)
        {
            SkillTestsContainer.Children.Add(new TextBlock
            {
                Text   = $"• {test.Name}: {test.Score}%",
                Margin = new Thickness(0, 5, 0, 5),
            });
        }
    }

    private static Uri GetUri(string? url, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(url) &&
            Uri.TryCreate(url, UriKind.Absolute, out var result) &&
            (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
            return result;
        return new Uri(fallback);
    }
}
