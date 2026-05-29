// <copyright file="TestPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews.Views
{
    using System;
    using System.Linq;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using Tests_and_Interviews.Repositories;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.ViewModels;

    /// <summary>
    /// TestPage is the main page where candidates take their tests.
    /// It handles loading the test, managing the timer, submitting answers, and showing the score and summary leaderboard after submission.
    /// </summary>
    public sealed partial class TestPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestPage"/> class.
        /// The constructor initializes the TestPage and sets up the ViewModel. It also calls InitializeComponent to load the XAML UI components
        /// defined for this page.
        /// </summary>
        public TestPage()
        {
            this.InitializeComponent();
            var userService = new UserService();
            var questionService = new QuestionService();
            var gradingService = new GradingService();
            var timerService = new TimerService();
            var validationService = new AttemptValidationService();
            var dataProcessingService = new DataProcessingService();
            var testService = new TestService();
            var leaderboardService = new LeaderboardService();
            this.LeaderboardViewModel = new LeaderboardViewModel(leaderboardService);
            this.ViewModel = new TestPageViewModel(userService, questionService, testService, dataProcessingService);
        }

        /// <summary>
        /// Gets the ViewModel for the TestPage, which contains all the logic and data for managing the test-taking process, including loading
        /// test details, tracking user answers, managing the timer, and submitting results.
        /// </summary>
        public TestPageViewModel ViewModel { get; }

        /// <summary>
        /// Gets the ViewModel for the LeaderboardView.
        /// </summary>
        public LeaderboardViewModel LeaderboardViewModel { get; }

        /// <summary>
        /// OnNavigatedTo is called when the page is navigated to.
        /// It checks if the navigation parameter contains the necessary information to load the test.
        /// </summary>
        /// <param name="eventArguments">The navigation event arguments, which should contain a TestNavigationArgs object with the test and user IDs.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs eventArguments)
        {
            base.OnNavigatedTo(eventArguments);

            if (eventArguments.Parameter is TestNavigationArgs args)
            {
                this.ViewModel.OnTimerExpired = async () =>
                {
                    await this.ViewModel.SubmitAsync();
                    this.ShowScoreDialog(0f, expired: true);
                };

                await this.ViewModel.LoadAsync(args.TestId, args.UserId);

                if (this.ViewModel.AlreadyAttempted)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Test unavailable",
                        Content = "You have already attempted this test. Each test can only be taken once.",
                        CloseButtonText = "Back to Tests",

                        XamlRoot = App.MainWindow.Content.XamlRoot,
                    };
                    await dialog.ShowAsync();
                    this.Frame.Navigate(typeof(MainTestPage));
                }
            }
        }

        /// <summary>
        /// BackToTests_Click is the event handler for the "Back to Tests" button click event.
        /// </summary>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="eventArguments">The event arguments associated with the button click event.</param>
        private void BackToTests_Click(object sender, RoutedEventArgs eventArguments)
        {
            this.ViewModel.StopTimer();
            this.Frame.Navigate(typeof(MainTestPage));
        }

        /// <summary>
        /// SubmitTest_Click is the event handler for the "Submit Test" button click event.
        /// </summary>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="eventArguments">The event arguments associated with the button click event.</param>
        private async void SubmitTest_Click(object sender, RoutedEventArgs eventArguments)
        {
            var dialog = new ContentDialog
            {
                Title = "Submit Test",
                Content = "Are you sure you want to submit? You cannot change your answers afterwards.",
                PrimaryButtonText = "Submit",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            float score = await this.ViewModel.SubmitAsync();
            this.ShowScoreDialog(score);
        }

        /// <summary>
        /// ShowScoreDialog displays a dialog showing the user's score after submitting the test.
        /// If the test was submitted due to timer expiration, it also shows a message indicating that time's up.
        /// </summary>
        /// <param name="score">The score achieved by the user on the test, typically a value between 0 and 100.</param>
        /// <param name="expired">A boolean flag indicating whether the test was submitted due to timer expiration. If true, a message about time expiration will be shown.</param>
        private void ShowScoreDialog(float score, bool expired = false)
        {
            var panel = new StackPanel { Spacing = 12 };

            if (expired)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = "Time's up! Your test was automatically submitted.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.OrangeRed),
                });
            }

            panel.Children.Add(new TextBlock
            {
                Text = $"Your score: {score:F1} / 100",
                FontSize = 28,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255)),
            });

            var scoreDialog = new ContentDialog
            {
                Title = "Test Completed!",
                Content = panel,
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot,
            };

            scoreDialog.CloseButtonClick += async (s, e) =>
            {
                await this.ShowSummaryLeaderboardDialogAsync();
            };

            _ = scoreDialog.ShowAsync();
        }

        /// <summary>
        /// ShowSummaryLeaderboardDialogAsync displays a summary leaderboard dialog after the user submits their test.
        /// It shows the top 3 performers for the test and the current user's ranking if they are not in the top 3. The user can then choose to see the full leaderboard or return to the main tests page.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation of showing the summary leaderboard dialog.</returns>
        private async System.Threading.Tasks.Task ShowSummaryLeaderboardDialogAsync()
        {
            var topThree = await this.LeaderboardViewModel.GetTopThreeAsync(this.ViewModel.TestId);
            var currentUserEntry = await this.LeaderboardViewModel.GetUserRankingAsync(App.CurrentUserId, this.ViewModel.TestId);
            var panel = new StackPanel { Spacing = 10 };

            panel.Children.Add(new TextBlock
            {
                Text = "Top 3 for this test",
                FontSize = 20,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 26, 26, 46)),
            });

            foreach (var entry in topThree)
            {
                panel.Children.Add(
                    this.CreateSummaryEntryCard(
                        entry.RankPosition,
                        entry.User?.Name ?? "Unknown user",
                        entry.NormalizedScore,
                        entry.UserId == App.CurrentUserId));
            }

            bool currentUserInTopThree = currentUserEntry != null &&
                                         topThree.Any(e => e.UserId == currentUserEntry.UserId);

            if (currentUserEntry != null && !currentUserInTopThree)
            {
                panel.Children.Add(new Border
                {
                    Height = 1,
                    Margin = new Thickness(0, 8, 0, 8),
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 232, 228, 255)),
                });

                panel.Children.Add(new TextBlock
                {
                    Text = "Your position",
                    FontSize = 16,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Margin = new Thickness(0, 10, 0, 6),
                });

                panel.Children.Add(
                    this.CreateSummaryEntryCard(
                        currentUserEntry.RankPosition,
                        currentUserEntry.User?.Name ?? "You",
                        currentUserEntry.NormalizedScore,
                        true));
            }

            var dialog = new ContentDialog
            {
                Title = "Summary Leaderboard",
                Content = panel,
                PrimaryButtonText = "See Full Leaderboard",
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot,
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                this.Frame.Navigate(typeof(LeaderboardPage), this.ViewModel.TestId);
            }
            else
            {
                this.Frame.Navigate(typeof(MainTestPage));
            }
        }

        /// <summary>
        /// CreateSummaryEntryCard creates a UI card for displaying a single leaderboard entry in the summary dialog.
        /// It shows the user's rank, name, and score, and highlights the current user's entry if applicable.
        /// </summary>
        /// <param name="rank">The rank position of the user in the leaderboard (e.g., 1 for first place).</param>
        /// <param name="name">The name of the user associated with this leaderboard entry.</param>
        /// <param name="score">The normalized score (percentage) achieved by the user on the test.</param>
        /// <param name="isCurrentUser">A boolean flag indicating whether this entry corresponds to the current user. If true, the card will be styled differently to highlight it.</param>
        /// <returns>A Border element representing the styled card for the leaderboard entry, containing the rank, name, and score information.</returns>
        private Border CreateSummaryEntryCard(int rank, string name, decimal score, bool isCurrentUser = false)
        {
            var border = new Border
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    isCurrentUser
                        ? Microsoft.UI.ColorHelper.FromArgb(255, 238, 234, 255)
                        : Microsoft.UI.Colors.White),
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    isCurrentUser
                        ? Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255)
                        : Microsoft.UI.ColorHelper.FromArgb(255, 232, 228, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 8),
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });

            var rankText = new TextBlock
            {
                Text = rank == 1 ? "🥇"
                     : rank == 2 ? "🥈"
                     : rank == 3 ? "🥉"
                     : $"#{rank}",
                FontSize = rank <= 3 ? 22 : 18,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255)),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(rankText, 0);

            var nameText = new TextBlock
            {
                Text = name,
                FontSize = 15,
                FontWeight = isCurrentUser
                    ? Microsoft.UI.Text.FontWeights.SemiBold
                    : Microsoft.UI.Text.FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
            };
            Grid.SetColumn(nameText, 1);

            var scoreText = new TextBlock
            {
                Text = $"{score:F1}%",
                FontSize = 15,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(scoreText, 2);

            grid.Children.Add(rankText);
            grid.Children.Add(nameText);
            grid.Children.Add(scoreText);

            border.Child = grid;
            return border;
        }
    }
}