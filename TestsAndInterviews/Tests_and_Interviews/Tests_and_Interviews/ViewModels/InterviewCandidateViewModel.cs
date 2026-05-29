// <copyright file="InterviewCandidateViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Tests_and_Interviews.Helpers;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// ViewModel for managing interview candidate interactions.
    /// </summary>
    public partial class InterviewCandidateViewModel : INotifyPropertyChanged
    {
        private readonly IInterviewSessionService sessionService;
        private readonly INotificationService notificationService;

        private string questionText;
        private List<Question> questions = new List<Question>();
        private int currentQuestionIndex = 0;
        private InterviewSession? session;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewCandidateViewModel"/> class, configuring the repositories and
        /// notification service required to manage interview sessions and candidate interactions.
        /// </summary>
        /// <remarks>This constructor sets up the commands for navigating interview questions and
        /// submitting recordings. After construction, call InitializeAsync to prepare the view model for use.</remarks>
        /// <param name="sessionService">The interview session service used to manage session data and business logic.</param>
        /// <param name="notificationService">The service used to send notifications to the candidate regarding interview updates.</param>
        public InterviewCandidateViewModel(
            IInterviewSessionService sessionService,
            INotificationService notificationService)
        {
            this.sessionService = sessionService;
            this.notificationService = notificationService;
            this.questionText = "Questions will start after starting recording";
            this.NextQuestionCommand = new RelayCommand(this.NextQuestion);
            this.SubmitRecordingCommand = new RelayCommand(this.SubmitRecording);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <remarks>This event is typically used to notify subscribers, such as user interface elements,
        /// that a property value has changed. It is commonly raised in data binding scenarios to enable automatic UI
        /// updates when underlying data changes. Implementers should raise this event whenever a property value is
        /// modified to ensure that all listeners are informed of the change.</remarks>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the command that advances to the next question in the quiz.
        /// </summary>
        /// <remarks>This command is typically bound to a user interface element, allowing users to
        /// navigate through questions sequentially. It may trigger validation or state changes before proceeding to the
        /// next question.</remarks>
        public ICommand NextQuestionCommand { get; }

        /// <summary>
        /// Gets the command that submits the recording for processing.
        /// </summary>
        /// <remarks>This command is typically bound to a user interface element, such as a button,
        /// allowing users to initiate the submission of a recording. Ensure that the command is executed in a valid
        /// state to avoid any exceptions.</remarks>
        public ICommand SubmitRecordingCommand { get; }

        /// <summary>
        /// Gets or sets the file path where the recording is saved.
        /// </summary>
        /// <remarks>The file path must be a valid path on the file system. If the path is invalid or
        /// inaccessible, an exception may be thrown when attempting to save the recording.</remarks>
        public string? RecordingFilePath { get; set; }

        /// <summary>
        /// Gets or sets the text of the interview question.
        /// </summary>
        /// <remarks>Setting this property raises a property change notification, which is useful for data
        /// binding scenarios in UI frameworks such as WPF or Xamarin.Forms.</remarks>
        public string QuestionText
        {
            get => this.questionText;
            set
            {
                if (this.questionText != value)
                {
                    this.questionText = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Asynchronously loads and initializes data for the specified interview session.
        /// </summary>
        /// <remarks>Call this method before performing any operations that require interview session data
        /// to ensure the data is properly initialized.</remarks>
        /// <param name="interviewSessionId">The unique identifier of the interview session to load. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation of loading the interview session data.</returns>
        public async Task LoadData(int interviewSessionId)
        {
            await this.InitializeAsync(interviewSessionId);
        }

        /// <summary>
        /// Begins the process of presenting questions to the user by retrieving and displaying the next available
        /// question.
        /// </summary>
        /// <remarks>Call this method to initialize or restart the question sequence. The method updates
        /// the internal state with the next question, which can then be used for user interaction. Ensure that the
        /// question retrieval logic is properly configured to avoid unexpected behavior.</remarks>
        public void StartQuestions()
        {
            this.QuestionText = this.GetNextQuestion();
        }

        /// <summary>
        /// Resets the question sequence to the initial state.
        /// </summary>
        public void ResetQuestions()
        {
            this.currentQuestionIndex = 0;
            this.QuestionText = "Questions will start after starting recording";
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property, notifying subscribers that a property value has
        /// changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. This value is optional and, if not provided, the name of the calling
        /// member is used.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Initializes the interview session asynchronously using the specified session identifier.
        /// </summary>
        /// <remarks>If the session is found, its start date is set to the current UTC time, and the
        /// associated interview questions are loaded. Any errors encountered during initialization are logged for
        /// debugging purposes.</remarks>
        /// <param name="interviewSessionId">The unique identifier of the interview session to initialize.</param>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        private async Task InitializeAsync(int interviewSessionId)
        {
            try
            {
                var result = await this.sessionService.StartSessionAsync(interviewSessionId);
                this.session = result.Session;
                this.questions = result.Questions;
                this.currentQuestionIndex = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InterviewCandidateViewModel.InitializeAsync failed: {ex.Message}");
                this.QuestionText = "An error occurred while loading the session.";
            }
        }

        /// <summary>
        /// Advances to the next question and updates the current question text.
        /// </summary>
        /// <remarks>This method is typically called to proceed to the subsequent question in a quiz or
        /// interview workflow. Ensure that the question retrieval logic in GetNextQuestion returns a valid question to
        /// avoid unexpected behavior.</remarks>
        private void NextQuestion()
        {
            this.QuestionText = this.GetNextQuestion();
        }

        /// <summary>
        /// Retrieves the text of the next question in the sequence, or a completion message if all questions have been
        /// answered.
        /// </summary>
        /// <returns>A string containing the text of the next unanswered question, or a message indicating that all questions
        /// have been completed.</returns>
        private string GetNextQuestion()
        {
            if (this.questions == null || this.currentQuestionIndex >= this.questions.Count)
            {
                return "Congratulation! You finnished all the questions. You may stop and submit the recording now.";
            }

            return this.questions[this.currentQuestionIndex++].QuestionText;
        }

        /// <summary>
        /// Submits the recorded video for the current interview session and updates the session status to in progress.
        /// </summary>
        /// <remarks>This method uploads the video file specified by the RecordingFilePath property to the
        /// interview session. If the upload is successful, a notification is displayed to the user. If the notification
        /// fails to show, a debug message is printed. If the upload fails, a debug message is also printed. Ensure that
        /// the session is not null before calling this method.</remarks>
        private async void SubmitRecording()
        {
            if (this.session == null)
            {
                this.questionText = "No session loaded. Cannot submit recording.";
                return;
            }

            try
            {
                await this.sessionService.SubmitRecordingAsync(this.session, this.RecordingFilePath ?? string.Empty);
                try
                {
                    this.notificationService.ShowSimpleNotification("Video uploaded", "Your interview video was uploaded successfully.");
                }
                catch
                {
                    this.QuestionText = "Video uploaded, but failed to show notification.";
                    Debug.Print("Failed to show notification, but video was uploaded successfully.");
                }
            }
            catch
            {
                Debug.Print("Failed to upload video. Please try again.");
                this.QuestionText = "Failed to upload video. Please try again.";
            }
        }
    }
}