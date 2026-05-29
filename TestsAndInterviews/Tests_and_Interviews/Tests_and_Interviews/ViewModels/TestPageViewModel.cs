// <copyright file="TestPageViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.UI.Xaml;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Helpers;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// TestPageViewModel is the main view model for the test page. It manages the state of the test, including the list of questions, the timer, and the user's answers.
    /// </summary>
    public class TestPageViewModel : INotifyPropertyChanged
    {
        private readonly IUserService userService;
        private readonly IQuestionService questionService;
        private readonly ITestService testService;
        private readonly IDataProcessingService dataProcessingService;
        private string testTitle = string.Empty;
        private TimeSpan timeLeft = TimeSpan.FromMinutes(TestConstants.TestDurationInMinutes);
        private DispatcherTimer? timer;
        private int answeredCount;
        private int attemptId;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestPageViewModel"/> class.
        /// This constructor sets up the necessary repositories and services for managing the test data and logic.
        /// </summary>
        /// <param name="userService">The repository for managing users in the database.</param>
        /// <param name="testRepository">The repository for managing tests in the database.</param>
        /// <param name="questionService">The repository for managing questions in the database.</param>
        /// <param name="attemptRepository">The repository for managing test attempts in the database.</param>
        /// <param name="answerRepository">The repository for managing answers in the database.</param>
        /// <param name="testService">The service for handling test-related business logic, such as starting and submitting tests.</param>
        /// <param name="dataProcessingService">The service for processing finalized test attempts, such as calculating scores and generating reports.</param>
        public TestPageViewModel(
            IUserService userService,
            IQuestionService questionService,
            ITestService testService,
            IDataProcessingService dataProcessingService)
        {
            this.userService = userService;
            this.questionService = questionService;
            this.testService = testService;
            this.dataProcessingService = dataProcessingService;
        }

        /// <summary>
        /// PropertyChanged event is raised whenever a property value changes. This allows the UI to automatically update when the underlying data changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the collection of questions for the test. This collection is used to display the questions in the UI and to track the user's answers.
        /// </summary>
        public ObservableCollection<QuestionViewModel> Questions { get; } = [];

        /// <summary>
        /// Gets or sets the title of the test. This is displayed at the top of the test page to indicate which test the user is taking.
        /// </summary>
        public string TestTitle
        {
            get => this.testTitle;
            set
            {
                this.testTitle = value;
                this.Notify();
            }
        }

        /// <summary>
        /// Gets a string representation of the remaining time for the test in the format "mm:ss". This is used to display the countdown timer to the user during the test.
        /// </summary>
        public string TimerDisplay => this.timeLeft.ToString(@"mm\:ss");

        /// <summary>
        /// Gets or sets an action that will be called when the timer expires.
        /// This allows the parent view model to react to the timer expiring, such as automatically submitting the test or showing a warning to the user.
        /// </summary>
        public Action? OnTimerExpired { get; set; }

        /// <summary>
        /// Gets or sets the count of how many questions have been answered by the user.
        /// This is used to track the user's progress through the test and can be displayed in the UI to show how many questions have been completed.
        /// </summary>
        public int AnsweredCount
        {
            get => this.answeredCount;
            set
            {
                this.answeredCount = value;
                this.Notify();
            }
        }

        /// <summary>
        /// Gets the total count of questions in the test. This is used to determine how many questions are in the test and can be displayed in the UI to show the total number of questions.
        /// </summary>
        public int TotalCount => this.Questions.Count;

        /// <summary>
        /// Gets a value indicating whether the user has already attempted the test.
        /// This is used to prevent users from taking the same test multiple times and can be used to display a message in the UI if they have already attempted it.
        /// </summary>
        public bool AlreadyAttempted { get; private set; } = false;

        /// <summary>
        /// Gets or sets the unique identifier for the user taking the test. This is used to associate the user's answers and test attempt with their account in the system.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the test being taken. This is used to load the correct questions and to associate the user's attempt with the specific test
        /// in the system.
        /// </summary>
        public int TestId { get; set; }

        /// <summary>
        /// Async method to load the test data for a given test ID and user ID. This method retrieves the test information, questions, and initializes the timer for the test.
        /// </summary>
        /// <param name="testId">The unique identifier for the test to be loaded.</param>
        /// <param name="userId">The unique identifier for the user taking the test. If not provided, it defaults to a user named "Alice Johnson".</param>
        /// <returns>A task that represents the asynchronous operation of loading the test data.</returns>
        public async System.Threading.Tasks.Task LoadAsync(int testId, int userId)
        {
            this.TestId = testId;

            if (userId > 0)
            {
                this.UserId = userId;
            }
            else
            {
                var users = await this.userService.GetAllAsync();
                var user = users.FirstOrDefault(user => user.Name == "Alice Johnson");
                this.UserId = user?.Id ?? 0;
            }

            System.Diagnostics.Debug.WriteLine($"[TestPageViewModel] UserId = {this.UserId}");

            var test = await this.testService.FindByIdAsync(testId);
            if (test == null)
            {
                return;
            }

            this.TestTitle = test.Title;

            try
            {
                await this.testService.StartTestAsync(this.UserId, testId);
            }
            catch (InvalidOperationException)
            {
                this.AlreadyAttempted = true;
                return;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"[StartTest error] {exception.InnerException?.Message ?? exception.Message}");
            }

            var questions = await this.questionService.FindByTestIdAsync(testId);

            int indexQuestion = 1;
            foreach (var question in questions)
            {
                if (question.Type == QuestionType.INTERVIEW)
                {
                    continue;
                }

                var questionViewModel = new QuestionViewModel
                {
                    QuestionId = question.Id,
                    DisplayNumber = indexQuestion++,
                    QuestionText = question.QuestionText,
                    Type = question.Type,
                };

                if (question.Type == QuestionType.SINGLE_CHOICE || question.Type == QuestionType.MULTIPLE_CHOICE)
                {
                    List<string> optionLabels;
                    if (!string.IsNullOrEmpty(question.OptionsJson))
                    {
                        optionLabels = System.Text.Json.JsonSerializer.Deserialize<List<string>>(question.OptionsJson)
                                       ?? ["Option A", "Option B", "Option C", "Option D", "Option E", "Option F"];
                    }
                    else
                    {
                        optionLabels = ["Option A", "Option B", "Option C", "Option D", "Option E", "Option F"];
                    }

                    for (int indexOption = 0; indexOption < optionLabels.Count; indexOption++)
                    {
                        var optionLabel = new OptionViewModel
                        {
                            Text = optionLabels[indexOption],
                            Index = indexOption,
                            GroupName = $"q_{question.Id}",
                            OnSelectionChanged = this.UpdateAnsweredCount,
                        };
                        questionViewModel.Options.Add(optionLabel);
                    }
                }

                questionViewModel.OnAnswerChanged = this.UpdateAnsweredCount;
                this.Questions.Add(questionViewModel);
            }

            this.Notify(nameof(this.TotalCount));
            this.StartTimer();
        }

        /// <summary>
        /// StopTimer stops the DispatcherTimer if it is currently running.
        /// This is typically called when the user submits the test or when the timer expires to prevent further updates to the remaining time.
        /// </summary>
        public void StopTimer()
        {
            this.timer?.Stop();
        }

        /// <summary>
        /// SubmitAsync collects the user's answers and delegates submission and processing to the test service.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation of submitting the test. The task result is the final score of the test attempt.</returns>
        public async System.Threading.Tasks.Task<float> SubmitAsync()
        {
            this.StopTimer();

            var answers = new List<AnswerDto>();

            foreach (var questionViewModel in this.Questions)
            {
                var answerValue = questionViewModel.GetAnswerValue();
                if (string.IsNullOrEmpty(answerValue))
                {
                    continue;
                }

                answers.Add(new AnswerDto
                {
                    QuestionId = questionViewModel.QuestionId,
                    Value = answerValue,
                    AttemptId = this.attemptId,
                });
            }

            return await this.testService.SubmitAttemptAsync(this.UserId, this.TestId, answers);
        }

        /// <summary>
        /// Gets or sets the collection of questions for the test. This collection is used to display the questions in the UI and to track the user's answers.
        /// </summary>
        /// <param name="property">The name of the property that changed. This parameter is optional and will be automatically filled by the compiler if not provided.</param>
        private void Notify([CallerMemberName] string property = "") =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        /// <summary>
        /// StartTimer initializes and starts a DispatcherTimer that counts down from the specified test duration.
        /// The timer updates the remaining time every second and raises an event when the time expires.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void StartTimer()
        {
            try
            {
                this.timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1),
                };
                this.timer.Tick += (dispatcherTimer, tickEventArguments) =>
                {
                    this.timeLeft = this.timeLeft.Subtract(TimeSpan.FromSeconds(1));
                    this.Notify(nameof(this.TimerDisplay));
                    if (this.timeLeft <= TimeSpan.Zero)
                    {
                        this.timer.Stop();
                        this.OnTimerExpired?.Invoke();
                    }
                };
                this.timer.Start();
            }
            catch
            {
            }
        }

        /// <summary>
        /// UpdateAnsweredCount calculates the number of questions that have been answered by the user and updates the AnsweredCount property.
        /// </summary>
        private void UpdateAnsweredCount()
        {
            this.AnsweredCount = this.Questions.Count(question => question.IsAnswered());
        }
    }
}