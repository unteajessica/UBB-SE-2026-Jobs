// <copyright file="InterviewInterviewerViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using Tests_and_Interviews.Helpers;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// ViewModel for managing the interviewer's view during an interview session.
    /// </summary>
    public partial class InterviewInterviewerViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Represents the service used to manage interview session data.
        /// </summary>
        private readonly IInterviewSessionService sessionService;

        /// <summary>
        /// Represents the service used to send notifications.
        /// </summary>
        private readonly INotificationService notificationService;

        /// <summary>
        /// Represents the ID of the current interview session being managed by this ViewModel.
        /// It is set during session initialization and used for fetching and updating session data.
        /// </summary>
        private int sessionId;

        /// <summary>
        /// Represents the URI of the recording resource.
        /// </summary>
        private Uri recordingUri;

        /// <summary>
        /// Gets or sets the score associated with the current instance.
        /// </summary>
        private float score;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewInterviewerViewModel"/> class.
        /// InterviewInterviewerViewModel initializes the InterviewSessionService and sets default values for RecordingUri and Score.
        /// </summary>
        /// <param name="sessionService">The service used to manage interview session data.</param>
        /// <param name="notificationService">The service used to send notifications.</param>
        public InterviewInterviewerViewModel(
            IInterviewSessionService sessionService,
            INotificationService notificationService)
        {
            this.sessionService = sessionService;
            this.notificationService = notificationService;
            this.SubmitScoreCommand = new RelayCommand(parameter => this.SubmitScore());

            this.recordingUri = new Uri("about:blank");
            this.score = 1.0f;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>This event is typically used in data binding scenarios to notify subscribers that a
        /// property has changed, allowing them to update the UI or perform other actions in response to the
        /// change.</remarks>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the command that submits the score.
        /// </summary>
        /// <remarks>This command is typically bound to a user interface element, such as a button, to
        /// trigger the score submission process.</remarks>
        public ICommand SubmitScoreCommand { get; }

        /// <summary>
        /// Gets or sets the URI of the recording.
        /// </summary>
        /// <remarks>Changing the value of this property raises the PropertyChanged event, notifying
        /// listeners of the update. This property is typically used to specify or retrieve the location where the
        /// recording is stored or accessed.</remarks>
        public Uri RecordingUri
        {
            get => this.recordingUri;
            set
            {
                if (this.recordingUri != value)
                {
                    this.recordingUri = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the score value that influences the overall performance evaluation.
        /// </summary>
        /// <remarks>The score is updated only when the new value differs from the current value, which
        /// triggers a property change notification. This behavior ensures that change notifications are not raised
        /// unnecessarily.</remarks>
        public float Score
        {
            get => this.score;
            set
            {
                if (this.score != value)
                {
                    this.score = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Initializes the interview session with the specified session identifier and sets the recording URI based on
        /// the session's video url.
        /// </summary>
        /// <remarks>If the video path associated with the session is null, empty, or invalid, the
        /// recording URI is set to 'about:blank'. This method is asynchronous but does not return a Task; callers
        /// should be aware that exceptions are handled internally and the method does not provide completion
        /// notification.</remarks>
        /// <param name="interviewSessionId">The unique identifier of the interview session to initialize.</param>
        public async void InitializeSession(int interviewSessionId)
        {
            this.sessionId = interviewSessionId;
            try
            {
                var session = await this.sessionService.GetSessionAsync(interviewSessionId);
                string videoUrl = session?.Video ?? string.Empty;

                if (string.IsNullOrWhiteSpace(videoUrl))
                {
                    this.RecordingUri = new Uri("about:blank");

                    return;
                }

                this.RecordingUri = new Uri(videoUrl);
            }
            catch
            {
                this.RecordingUri = new Uri("about:blank");
            }
        }

        /// <summary>
        /// Submits the interview score for the current session and updates its status to completed.
        /// </summary>
        /// <remarks>If the score submission is successful, a notification is displayed to the user. If
        /// the notification fails to show, a message is logged to the debug output. In case of an error during the
        /// score submission process, an error message is logged.</remarks>
        public async void SubmitScore()
        {
            try
            {
                await this.sessionService.SubmitScoreAsync(this.sessionId, this.Score);
                try
                {
                    this.notificationService.ShowSimpleNotification("Score submitted", "The interview score was submitted successfully.");
                }
                catch
                {
                    Debug.WriteLine("Failed to show notification, but score was submitted successfully.");
                }
            }
            catch
            {
                Debug.WriteLine("Failed to submit score. Please try again.");
            }
        }

        /// <summary>
        /// Triggers the PropertyChanged event for the specified property name, allowing subscribers to be notified of changes to property values.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}