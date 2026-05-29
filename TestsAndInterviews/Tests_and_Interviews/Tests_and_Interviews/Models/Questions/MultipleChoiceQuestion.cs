namespace Tests_and_Interviews.Models.Questions
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// MultipleChoiceQuestion class represents a specific type of question that allows for multiple options and multiple correct answers.
    /// </summary>
    public class MultipleChoiceQuestion : Question
    {
        /// <summary>
        /// Gets or sets the list of options for the multiple-choice question. This property is not mapped to the database and can be used
        /// to temporarily store the options during the question creation or editing process.
        /// </summary>
        [NotMapped]
        public List<string> Options { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of indexes corresponding to the correct answers for the multiple-choice question.
        /// This property is not mapped to the database and can be used
        /// </summary>
        [NotMapped]
        public List<int> CorrectAnswerIndexes { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of indexes corresponding to the options selected by the user for the multiple-choice question.
        /// </summary>
        [NotMapped]
        public List<int> SelectedIndexes { get; set; } = [];
    }
}