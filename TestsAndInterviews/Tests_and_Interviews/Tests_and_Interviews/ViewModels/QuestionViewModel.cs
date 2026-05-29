namespace Tests_and_Interviews.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.UI.Xaml;
    using Tests_and_Interviews.Helpers;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// QuestionViewModel represents a single question in the test. 
    /// It includes properties for the question text, type, and answer options, as well as methods to get the user's answer and check if the question has been answered.
    /// </summary>
    public partial class QuestionViewModel : INotifyPropertyChanged
    {
        private string textAnswer = string.Empty;
        private bool falseSelected;
        private bool trueSelected;

        /// <summary>
        /// PropertyChanged event is raised whenever a property value changes. This allows the UI to automatically update when the underlying data changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the unique identifier for the question. This ID is used to associate the question with its corresponding answer when the user submits their responses.
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// Gets or sets the display number for the question, which is used to show the question's position in the test (e.g., "Question 1", "Question 2", etc.).
        /// </summary>
        public int DisplayNumber { get; set; }

        /// <summary>
        /// Gets or sets the text of the question. This is the main content of the question that will be presented to the user, describing what they need to answer.
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the question, which determines how the question will be displayed and how the user's answer will be processed.
        /// </summary>
        public QuestionType Type { get; set; }

        /// <summary>
        /// Gets a user-friendly string representation of the question type.
        /// It converts the enum value to a string and replaces underscores with spaces for better readability in the UI.
        /// </summary>
        public string TypeLabel => this.Type.ToString().Replace("_", " ");

        /// <summary>
        /// Gets or sets the collection of answer options for the question.
        /// This is used for single-choice and multiple-choice questions to store the available options that the user can select from.
        /// </summary>
        public ObservableCollection<OptionViewModel> Options { get; set; } = [];

        /// <summary>
        /// Gets visibility values for different question types to control which UI elements are shown based on the question type.
        /// </summary>
        public Visibility IsSingleChoice => this.Type == QuestionType.SINGLE_CHOICE ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets visibility values for different question types to control which UI elements are shown based on the question type.
        /// </summary>
        public Visibility IsMultipleChoice => this.Type == QuestionType.MULTIPLE_CHOICE ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets visibility values for different question types to control which UI elements are shown based on the question type.
        /// </summary>
        public Visibility IsTrueFalse => this.Type == QuestionType.TRUE_FALSE ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets visibility values for different question types to control which UI elements are shown based on the question type.
        /// </summary>
        public Visibility IsText => this.Type == QuestionType.TEXT ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets the group name for true/false options, which is used to ensure that the true and false options are mutually exclusive when displayed in the UI.
        /// </summary>
        public string TrueFalseGroup => $"tf_{this.QuestionId}";

        /// <summary>
        /// Gets or sets a value indicating whether the "True" option is selected for a true/false question.
        /// When this value is set to true, it automatically sets the "FalseSelected" property to false to ensure that only one of the two options can be selected at a time.
        /// It also raises the PropertyChanged event to update the UI and invokes the OnAnswerChanged action to notify any listeners that the answer has changed.
        /// </summary>
        public bool TrueSelected
        {
            get => this.trueSelected;
            set
            {
                this.trueSelected = value;
                if (value)
                {
                    this.falseSelected = false;
                }

                this.Notify();
                this.Notify(nameof(this.FalseSelected));
                this.OnAnswerChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the "False" option is selected for a true/false question.
        /// </summary>
        public bool FalseSelected
        {
            get => this.falseSelected;
            set
            {
                this.falseSelected = value;
                if (value)
                {
                    this.trueSelected = false;
                }

                this.Notify();
                this.Notify(nameof(this.TrueSelected));
                this.OnAnswerChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets the text answer for a free-text question. This property is used to store the user's input for questions that require a written response.
        /// </summary>
        public string TextAnswer
        {
            get => this.textAnswer;
            set { this.textAnswer = value;
                this.Notify();
                this.OnAnswerChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets an action that will be called whenever the user's answer to the question changes. This allows the parent view model to react to changes in the user's answer.        /// </summary>
        public Action? OnAnswerChanged { get; set; }

        /// <summary>
        /// Gets the user's answer in a standardized string format based on the question type.
        /// This method processes the user's selections or input and returns a string representation that can be easily stored and evaluated.
        /// </summary>
        /// <returns>A string containig the user's selection or input.</returns>
        public string GetAnswerValue()
        {
            return this.Type switch
            {
                QuestionType.SINGLE_CHOICE => this.Options.FirstOrDefault(o => o.IsSelected)?.Index.ToString() ?? string.Empty,
                QuestionType.MULTIPLE_CHOICE =>
                    "[" + string.Join(",", this.Options.Where(o => o.IsSelected).Select(o => o.Index)) + "]",
                QuestionType.TRUE_FALSE => this.TrueSelected ? "true" : this.FalseSelected ? "false" : string.Empty,
                QuestionType.TEXT => this.TextAnswer.Trim(),
                _ => string.Empty
            };
        }

        /// <summary>
        /// IsAnswered checks whether the user has provided an answer for the question based on its type. 
        /// It returns true if the user has made a selection or entered text, and false otherwise.
        /// </summary>
        /// <returns>True if the question has been answered, false otherwise.</returns>
        public bool IsAnswered()
        {
            return this.Type switch
            {
                QuestionType.SINGLE_CHOICE => this.Options.Any(o => o.IsSelected),
                QuestionType.MULTIPLE_CHOICE => this.Options.Any(o => o.IsSelected),
                QuestionType.TRUE_FALSE => this.TrueSelected || this.FalseSelected,
                QuestionType.TEXT => !string.IsNullOrWhiteSpace(this.TextAnswer),
                _ => false
            };
        }


        /// <summary>
        /// Notify method is a helper function that raises the PropertyChanged event for a given property name.
        /// </summary>
        /// <param name="property">The name of the property that changed. This parameter is optional and will be automatically filled by the compiler if not provided.</param>
        private void Notify([CallerMemberName] string property = "") =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}