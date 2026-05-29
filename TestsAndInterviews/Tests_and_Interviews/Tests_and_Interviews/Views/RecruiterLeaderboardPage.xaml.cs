namespace Tests_and_Interviews.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Json;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Navigation;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Mappers;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services;

    /// <summary>
    /// Represents the page that displays a paginated leaderboard of test attempts for recruiters.
    /// </summary>
    public sealed partial class RecruiterLeaderboardPage : Page
    {
        private const int PageSize = 10;
        private List<TestAttempt> entries = new List<TestAttempt>();
        private int currentPage = 1;
        private int testId;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecruiterLeaderboardPage"/> class.
        /// </summary>
        public RecruiterLeaderboardPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the page navigation event to load test and leaderboard data.
        /// </summary>
        /// <param name="e">The navigation event arguments containing the test identifier.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int testId)
            {
                this.testId = testId;
            }

            try
            {
                var testResponse = await ApiClient.Http.GetAsync($"tests/{this.testId}");
                testResponse.EnsureSuccessStatusCode();
                var testDto = await testResponse.Content.ReadFromJsonAsync<TestDto>();
                if (testDto != null)
                {
                    var test = testDto.ToEntity();
                    if (test != null)
                    {
                        this.PageTitleText.Text = test.Title;
                        this.PageSubtitleText.Text = "Detailed recruiter leaderboard view";
                    }
                }

                var attemptsResponse = await ApiClient.Http.GetAsync($"testattempts/valid/bytest/{this.testId}");
                if (attemptsResponse.IsSuccessStatusCode)
                {
                    var attemptDtos = await attemptsResponse.Content.ReadFromJsonAsync<List<TestAttemptDto>>();
                    if (attemptDtos != null)
                    {
                        this.entries = attemptDtos.Select(dto => dto.ToEntity()).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading leaderboard data: {ex.Message}");
            }

            this.RenderPage();
        }

        private void RenderPage()
        {
            this.LeaderboardPanel.Children.Clear();

            var pagedEntries = this.entries
                .Skip((this.currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            int rankBase = (this.currentPage - 1) * PageSize;

            for (int i = 0; i < pagedEntries.Count; i++)
            {
                var entry = pagedEntries[i];
                int rank = rankBase + i + 1;

                var border = new Border
                {
                    Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                    BorderBrush = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 232, 228, 255)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(20),
                };

                var panel = new StackPanel { Spacing = 8 };

                var topGrid = new Grid();
                topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                var rankText = new TextBlock
                {
                    Text = $"#{rank}",
                    FontSize = 18,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255)),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(rankText, 0);

                var nameText = new TextBlock
                {
                    Text = entry.User?.Name ?? "Unknown user",
                    FontSize = 16,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 26, 26, 46)),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(nameText, 1);

                var scoreText = new TextBlock
                {
                    Text = $"{entry.PercentageScore:F1}%",
                    FontSize = 16,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 26, 26, 46)),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(scoreText, 2);

                topGrid.Children.Add(rankText);
                topGrid.Children.Add(nameText);
                topGrid.Children.Add(scoreText);
                int durationMinutes = this.GetDurationMinutes(entry);

                var rawScoreText = new TextBlock
                {
                    Text = $"Raw score: {entry.Score:F1} / 100",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 110, 110, 110)),
                    TextWrapping = TextWrapping.Wrap,
                };

                var startedCompletedText = new TextBlock
                {
                    Text = $"Started at: {entry.StartedAt:dd/MM/yyyy HH:mm}    |    Completed at: {entry.CompletedAt:dd/MM/yyyy HH:mm}",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 110, 110, 110)),
                    TextWrapping = TextWrapping.Wrap,
                };

                var durationText = new TextBlock
                {
                    Text = $"Duration: {durationMinutes} min",
                    FontSize = 13,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 110, 110, 110)),
                    TextWrapping = TextWrapping.Wrap,
                };

                panel.Children.Add(topGrid);
                panel.Children.Add(rawScoreText);
                panel.Children.Add(startedCompletedText);
                panel.Children.Add(durationText);

                border.Child = panel;
                this.LeaderboardPanel.Children.Add(border);
            }

            int totalPages = Math.Max(1, (int)Math.Ceiling((double)this.entries.Count / PageSize));
            this.PageInfoText.Text = $"Page {this.currentPage} of {totalPages}";
            this.PrevButton.IsEnabled = this.currentPage > 1;
            this.NextButton.IsEnabled = this.currentPage < totalPages;
        }

        private bool AreMultipleChoiceAnswersEqual(string submitted, string correct)
        {
            var submittedSet = this.ParseAnswerIndexes(submitted);
            var correctSet = this.ParseAnswerIndexes(correct);

            return submittedSet.SetEquals(correctSet);
        }

        private HashSet<int> ParseAnswerIndexes(string value)
        {
            var result = new HashSet<int>();

            if (string.IsNullOrWhiteSpace(value))
            {
                return result;
            }

            var cleaned = value.Trim().TrimStart('[').TrimEnd(']');
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                return result;
            }

            foreach (var part in cleaned.Split(','))
            {
                if (int.TryParse(part.Trim(), out int index))
                {
                    result.Add(index);
                }
            }

            return result;
        }

        private int GetDurationMinutes(TestAttempt attempt)
        {
            if (attempt.CompletedAt == null || attempt.StartedAt == null)
            {
                return 0;
            }

            return (int)(attempt.CompletedAt.Value - attempt.StartedAt.Value).TotalMinutes;
        }

        private void BackToRecruiterTests_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(RecruiterTestsPage));
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentPage > 1)
            {
                this.currentPage--;
                this.RenderPage();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)this.entries.Count / PageSize));
            if (this.currentPage < totalPages)
            {
                this.currentPage++;
                this.RenderPage();
            }
        }
    }
}