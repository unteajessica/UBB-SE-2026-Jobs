namespace Tests_and_Interviews.Models.Questions
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// SingleChoiceQuestion class represents a specific type of question that allows for multiple options but only one correct answer.
    /// </summary>
    public class SingleChoiceQuestion : Question
    {
        /// <summary>
        /// Gets or sets the list of options for the single-choice question.
        /// This property is not mapped to the database and can be used to temporarily
        /// store the options during the question creation or editing process.
        /// </summary>
        [NotMapped]
        public List<string> Options { get; set; } = [];

        /// <summary>
        /// Gets or sets the index corresponding to the correct answer for the single-choice question.
        /// </summary>
        [NotMapped]
        public int CorrectAnswerIndex { get; set; }

        /// <summary>
        /// Gets or sets the index corresponding to the option selected by the user for the single-choice question.
        /// </summary>
        [NotMapped]
        public int? SelectedIndex { get; set; }
    }
}