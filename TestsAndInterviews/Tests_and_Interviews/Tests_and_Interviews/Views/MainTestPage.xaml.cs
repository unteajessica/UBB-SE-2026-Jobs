namespace Tests_and_Interviews.Views
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using System;
    using System.Linq;
    using Tests_and_Interviews.Repositories;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.ViewModels;

    /// <summary>
    /// MainTestPage serves as the landing page for candidates, displaying a list of available tests.
    /// Candidates can select a test to start, and the page also provides visual feedback when hovering over test cards.
    /// </summary>
    public sealed partial class MainTestPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainTestPage"/> class.
        /// </summary>
        public MainTestPage()
        {
            this.InitializeComponent();
            this.ViewModel = new MainTestViewModel(new TestService());
        }

        /// <summary>
        /// Gets the ViewModel for the MainTestPage, which contains the logic for loading and managing the list of tests displayed on the page.
        /// </summary>
        public MainTestViewModel ViewModel { get; }

        /// <summary>
        /// Overrides the OnNavigatedTo method to load the list of tests when the page is navigated to.
        /// This ensures that the latest tests are displayed to the candidate each time they visit the page.
        /// </summary>
        /// <param name="eventArguments">The navigation event arguments containing information about the navigation event.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs eventArguments)
        {
            base.OnNavigatedTo(eventArguments);
            await this.ViewModel.LoadTestsAsync();
        }

        /// <summary>
        /// Starts the selected test when the candidate clicks the "Start Test" button.
        /// It retrieves the test ID from the button's Tag property, updates the selected test in the ViewModel, and navigates to the TestPage with the appropriate navigation arguments.
        /// </summary>
        /// <param name="sender">The button that was clicked to start the test.</param>
        /// <param name="eventArguments">The event arguments for the click event.</param>
        private void StartTest_Click(object sender, RoutedEventArgs eventArguments)
        {
            if (sender is Button button && button.Tag != null)
            {
                int testId = Convert.ToInt32(button.Tag);

                var selected = this.ViewModel.Tests.FirstOrDefault(t => t.TestId == testId);
                if (selected != null)
                {
                    this.ViewModel.SelectedTest = selected;
                }

                this.Frame.Navigate(typeof(TestPage), new TestNavigationArgs
                {
                    TestId = testId,
                    UserId = App.CurrentUserId,
                });
            }
        }

        /// <summary>
        /// Card_PointerEntered and Card_PointerExited provide visual feedback when the user hovers over a test card.
        /// </summary>
        /// <param name="sender">The button representing the test card that the pointer entered or exited.</param>
        /// <param name="eventArguments">The event arguments for the pointer event.</param>
        private void Card_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs eventArguments)
        {
            if (sender is Button button && button.Tag != null)
            {
                int testId = Convert.ToInt32(button.Tag);
                var card = this.ViewModel.Tests.FirstOrDefault(t => t.TestId == testId);
                if (card != null)
                {
                    card.IsHovered = true;
                }
            }
        }

        /// <summary>
        /// Card_PointerExited resets the hover state of the test card when the pointer leaves the card area,
        /// ensuring that the visual feedback is consistent and intuitive for the user.
        /// </summary>
        /// <param name="sender">The button representing the test card that the pointer exited.</param>
        /// <param name="eventArguments">The event arguments for the pointer event.</param>
        private void Card_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs eventArguments)
        {
            if (sender is Button button && button.Tag != null)
            {
                int testId = Convert.ToInt32(button.Tag);
                var card = this.ViewModel.Tests.FirstOrDefault(t => t.TestId == testId);
                if (card != null)
                {
                    card.IsHovered = false;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainWindow.ReturnToMainMenu();
        }
    }
}