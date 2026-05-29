namespace Tests_and_Interviews.Models.Questions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// TrueFalseQuestion class represents a specific type of question that requires a boolean answer (true or false).
    /// </summary>
    public class TrueFalseQuestion : Question
    {
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets represents the correct answer for the true/false question.
        /// This property is not mapped to the database and can be used to temporarily 
        /// store the correct answer during the question creation or editing process.
        /// </summary>
        [NotMapped]
        public bool CorrectAnswer
        {
            get => bool.TryParse(this.QuestionAnswer, out var result) && result;
            set => this.QuestionAnswer = value.ToString().ToLower();
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets represents the user's answer for the true/false question.
        /// </summary>
        [NotMapped]
        public bool? UserAnswerBool { get; set; }
    }
}