namespace Tests_and_Interviews.Models.Core
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents an answer submitted for a specific question within a test attempt.
    /// </summary>
    /// <remarks>
    /// This class associates a user's response with both the related question and the test attempt
    /// in which it was provided. It is typically used in systems that track user progress or results for
    /// assessments.
    /// </remarks>
    [Table("Answers")]
    public class Answer
    {
        /// <summary>
        /// Gets or sets the unique identifier for the answer.
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the question.
        /// </summary>
        [Column("question_id")]
        public int QuestionId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the attempt.
        /// </summary>
        [Column("attempt_id")]
        public int AttemptId { get; set; }

        /// <summary>
        /// Gets or sets the value of the answer provided by the user for the associated question and attempt.
        /// </summary>
        [Column("value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the question associated with this instance.
        /// </summary>
        [ForeignKey("QuestionId")]
        public Question? Question { get; set; }

        /// <summary>
        /// Gets or sets the current test attempt associated with this instance.
        /// </summary>
        [ForeignKey("AttemptId")]
        public TestAttempt? TestAttempt { get; set; }
    }
}