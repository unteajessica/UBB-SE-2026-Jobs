namespace Tests_and_Interviews.Web.Dtos
{
    /// <summary>
    /// DTO used by the web project when rendering and submitting question data.
    /// Properties mirror those used by the API/service layers.
    /// </summary>
    public class QuestionDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the text of the question.
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the question.
        /// </summary>
        public string QuestionType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the score assigned to the question.
        /// </summary>
        public float QuestionScore { get; set; }

        /// <summary>
        /// Gets or sets the answer provided to the question.
        /// </summary>
        public string? QuestionAnswer { get; set; }

        /// <summary>
        /// Gets or sets the serialized options in JSON format.
        /// </summary>
        /// <remarks>Use this property to store or retrieve configuration options as a JSON string. The
        /// format and schema of the JSON should match the expected options structure for correct
        /// deserialization.</remarks>
        public string? OptionsJson { get; set; }

        /// <summary>
        /// The ID of the test this question belongs to, if any. 
        /// </summary>
        public int? TestId { get; set; }
    }
}
