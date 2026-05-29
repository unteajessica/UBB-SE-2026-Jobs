namespace Tests_and_Interviews.Views
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Navigation;
    using Tests_and_Interviews.Repositories;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.ViewModels;

    /// <summary>
    /// Represents the page that displays the global leaderboard for a specific test to the user.
    /// </summary>
    public sealed partial class LeaderboardPage : Page
    {
        private LeaderboardViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardPage"/> class.
        /// </summary>
        public LeaderboardPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        /// <param name="e">Event data that can be examined by overriding code. The parameter is typically the test ID.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int testId)
            {
                this.viewModel = new LeaderboardViewModel(
                    new LeaderboardService());

                await this.viewModel.LoadAsync(testId);
                this.RenderPage();
            }
        }

        private void RenderPage()
        {
            this.LeaderboardPanel.Children.Clear();

            foreach (var entry in this.viewModel.GetCurrentPageEntries())
            {
                bool isCurrentUser = entry.UserId == App.CurrentUserId;

                var border = new Border
                {
                    Background = new SolidColorBrush(
                        isCurrentUser
                            ? Microsoft.UI.ColorHelper.FromArgb(255, 238, 234, 255)
                            : Microsoft.UI.Colors.White),
                    BorderBrush = new SolidColorBrush(
                        isCurrentUser
                            ? Microsoft.UI.ColorHelper.FromArgb(255, 132, 148, 255)
                            : Microsoft.UI.ColorHelper.FromArgb(255, 232, 228, 255)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(20),
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

                var rankText = new TextBlock
                {
                    Text = $"#{entry.RankPosition}",
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
                    FontWeight = isCurrentUser
                        ? Microsoft.UI.Text.FontWeights.SemiBold
                        : Microsoft.UI.Text.FontWeights.Normal,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 26, 26, 46)),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(nameText, 1);

                var scoreText = new TextBlock
                {
                    Text = $"{entry.NormalizedScore:F1}%",
                    FontSize = 16,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, 26, 26, 46)),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(scoreText, 2);

                grid.Children.Add(rankText);
                grid.Children.Add(nameText);
                grid.Children.Add(scoreText);

                border.Child = grid;
                this.LeaderboardPanel.Children.Add(border);
            }

            this.PageInfoText.Text = $"Page {this.viewModel.CurrentPage} of {this.viewModel.TotalPages}";
            this.PrevButton.IsEnabled = this.viewModel.CanGoPrev;
            this.NextButton.IsEnabled = this.viewModel.CanGoNext;
        }

        private void BackToTests_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainTestPage));
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.GoToPrevPage();
            this.RenderPage();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.GoToNextPage();
            this.RenderPage();
        }
    }
}