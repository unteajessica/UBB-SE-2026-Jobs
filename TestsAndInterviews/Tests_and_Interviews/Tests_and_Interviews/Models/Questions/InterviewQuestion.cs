namespace Tests_and_Interviews.Models.Questions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// InterviewQuestion class represents a specific type of question that is used in interviews.
    /// </summary>
    public class InterviewQuestion : Question
    {

        /// <summary>
        /// Gets or sets additional notes or comments related to the interview question.
        /// This property is not mapped to the database and can be used for temporary
        /// storage of information during the interview process.
        /// </summary>
        [NotMapped]
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's response to the interview question.
        /// This property is not mapped to the database and can be used to temporarily
        /// store the candidate's answer during the interview process.
        /// </summary>
        [NotMapped]
        public string UserResponse { get; set; } = string.Empty;
    }
}