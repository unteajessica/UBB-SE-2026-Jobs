namespace Tests_and_Interviews.Views
{
    using System;
    using System.Diagnostics;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using Tests_and_Interviews.Repositories;
    using Tests_and_Interviews.ViewModels;
    using Windows.Globalization.NumberFormatting;
    using Windows.Media.Core;

    /// <summary>
    /// Interaction logic for InterviewInterviewerPage.xaml.
    /// </summary>
    public sealed partial class InterviewInterviewerPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewInterviewerPage"/> class.
        /// The constructor for the <see cref="InterviewInterviewerPage"/> class initializes the page, sets up the ViewModel,
        /// and configures the number formatter for the score input.
        /// </summary>
        public InterviewInterviewerPage()
        {
            this.InitializeComponent();
            var sessionService = new Services.InterviewSessionService();
            var notificationService = new Services.NotificationService(new Services.Interfaces.WindowsToastNotifier());
            this.ViewModel = new InterviewInterviewerViewModel(sessionService, notificationService);
            this.SetNumberBoxNumberFormatter();
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// Gets or sets the ViewModel for the InterviewInterviewerPage.
        /// </summary>
        public InterviewInterviewerViewModel ViewModel { get; set; }

        /// <summary>
        /// Creates a MediaSource from the given URI.
        /// </summary>
        /// <param name="uri">The URI of the media to create the MediaSource from.</param>
        /// <returns>A MediaSource if the URI is valid; otherwise, null.</returns>
        public MediaSource? WidegetMediaSource(Uri uri)
        {
            return uri != null ? MediaSource.CreateFromUri(uri) : null;
        }

        /// <summary>
        /// Called when the page is navigated to.
        /// </summary>
        /// <param name="e">The navigation event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int id && id > 0)
            {
                this.ViewModel.InitializeSession(id);
            }
        }

        /// <summary>
        /// Handles the click event for the SubmitScore button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void SubmitScore_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            this.ViewModel.SubmitScore();
            if (this.Tag is Window hostWindow)
            {
                try
                {
                    hostWindow.Close();
                    return;
                }
                catch
                {
                    Debug.WriteLine("Host window close threw an exception, but it will be ignored.");
                }
            }

            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void Skip10_Click(object sender, RoutedEventArgs e)
        {
            if (this.InterviewPlayer.MediaPlayer == null)
            {
                return;
            }

            var session = this.InterviewPlayer.MediaPlayer.PlaybackSession;
            session.Position += TimeSpan.FromSeconds(10);
        }

        private void SetNumberBoxNumberFormatter()
        {
            IncrementNumberRounder rounder = new IncrementNumberRounder
            {
                Increment = 0.01,
                RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp,
            };

            DecimalFormatter formatter = new DecimalFormatter
            {
                IntegerDigits = 1,
                FractionDigits = 2,
                NumberRounder = rounder,
            };
            this.FormattedNumberBox.NumberFormatter = formatter;
            this.FormattedNumberBox.Minimum = 1;
            this.FormattedNumberBox.Maximum = 10;
        }
    }
}