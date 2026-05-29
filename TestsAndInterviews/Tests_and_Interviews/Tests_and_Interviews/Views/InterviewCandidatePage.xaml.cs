namespace Tests_and_Interviews.Views
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Repositories;
    using Tests_and_Interviews.Services.Interfaces;
    using Tests_and_Interviews.ViewModels;
    using Windows.Media.Capture;
    using Windows.Media.MediaProperties;
    using Windows.Storage;

    /// <summary>
    /// Interaction logic for InterviewCandidatePage.xaml.
    /// </summary>
    public sealed partial class InterviewCandidatePage : Page
    {
        private MediaCapture? mediaCapture = new MediaCapture();

        private bool isRecording = false;

        private StorageFile? recordingFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewCandidatePage"/> class.
        /// </summary>
        public InterviewCandidatePage()
        {
            this.InitializeComponent();
            var sessionService = new Services.InterviewSessionService();
            var notificationService = new Services.NotificationService(new WindowsToastNotifier());
            this.ViewModel = new InterviewCandidateViewModel(sessionService, notificationService);
            this.DataContext = this.ViewModel;
            this.StopVideoButton.IsEnabled = false;
            this.SubmitVideoButton.IsEnabled = false;
            this.NextQuestionButton.IsEnabled = false;
            this.RecordingBorder.BorderThickness = new Thickness(0);
            this.StartCamera();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewCandidatePage"/> class with a specified interview session.
        /// </summary>
        /// <param name="session">The interview session.</param>
        public InterviewCandidatePage(InterviewSession session)
        {
            this.InterviewSession = session;
            var sessionService = new Services.InterviewSessionService();
            var notificationService = new Services.NotificationService(new WindowsToastNotifier());
            this.ViewModel = new InterviewCandidateViewModel(sessionService, notificationService);
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
            this.StopVideoButton.IsEnabled = false;
            this.SubmitVideoButton.IsEnabled = false;
            this.NextQuestionButton.IsEnabled = false;
            this.RecordingBorder.BorderThickness = new Thickness(0);
            _ = this.ViewModel.LoadData(session.Id);
            this.StartCamera();
        }

        /// <summary>
        /// Gets or sets the interview session.
        /// </summary>
        public InterviewSession? InterviewSession { get; set; }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        public InterviewCandidateViewModel ViewModel { get; }

        /// <summary>
        /// Gets or sets the action to be invoked when the page is closed.
        /// </summary>
        public Action? OnClosed { get; set; }

        private async void StartCamera()
        {
            if (this.mediaCapture == null)
            {
                return;
            }

            try
            {
                await this.mediaCapture.InitializeAsync();
            }
            catch
            {
                Debug.WriteLine("Media capture not loaded properly");
                return;
            }

            var frameSource = this.mediaCapture.FrameSources.Values.FirstOrDefault(
                source => source.Info.SourceKind == Windows.Media.Capture.Frames.MediaFrameSourceKind.Color);

            if (frameSource != null)
            {
                this.captureElement.Source = Windows.Media.Core.MediaSource.CreateFromMediaFrameSource(frameSource);
                this.captureElement.MediaPlayer.Play();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No valid color video frame source was found.");
            }
        }

        private async void StartRecording_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (this.isRecording || this.mediaCapture == null)
            {
                return;
            }

            try
            {
                this.StartVideoButton.IsEnabled = false;
                this.StopVideoButton.IsEnabled = true;
                this.SubmitVideoButton.IsEnabled = false;
                this.NextQuestionButton.IsEnabled = true;
                this.RecordingBorder.BorderThickness = new Thickness(10);

                var storageFolder = ApplicationData.Current.LocalFolder;
                this.recordingFile = await storageFolder.CreateFileAsync("CandidateInterview.mp4", CreationCollisionOption.ReplaceExisting);
                this.ViewModel.RecordingFilePath = this.recordingFile.Path;
                var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

                await this.mediaCapture.StartRecordToStorageFileAsync(encodingProfile, this.recordingFile);
                this.isRecording = true;
                System.Diagnostics.Debug.WriteLine($"Recording started: {this.recordingFile.Path}");

                this.ViewModel.StartQuestions();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to start recording: {ex.Message}");
            }
        }

        private async void StopRecording_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (!this.isRecording || this.mediaCapture == null)
            {
                return;
            }

            try
            {
                this.StopVideoButton.IsEnabled = false;
                this.StartVideoButton.IsEnabled = true;
                this.SubmitVideoButton.IsEnabled = true;
                this.RecordingBorder.BorderThickness = new Thickness(0);
                this.NextQuestionButton.IsEnabled = false;

                await this.mediaCapture.StopRecordAsync();
                this.isRecording = false;

                System.Diagnostics.Debug.WriteLine("Recording stopped successfully.");

                this.ViewModel.ResetQuestions();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to stop recording: {ex.Message}");
            }
        }

        private void ExitPage_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            this.mediaCapture?.Dispose();
            this.mediaCapture = null;

            try
            {
                this.OnClosed?.Invoke();
            }
            catch
            {
                Debug.WriteLine("OnClosed action threw an exception, but it will be ignored.");
            }

            if (this.Tag is Window hostWindow)
            {
                try
                {
                    hostWindow.Close();
                    return;
                }
                catch
                {
                    Debug.WriteLine("Failed to close host window, falling back to frame navigation.");
                }
            }

            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}