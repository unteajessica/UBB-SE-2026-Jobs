namespace Tests_and_Interviews.Models.Enums
{
    /// <summary>
    /// Specifies the types of questions that can be used in a survey or assessment.
    /// </summary>
    /// <remarks>
    /// Use this enumeration to indicate the format or expected response type for a question. The
    /// available types include single choice, multiple choice, free-text, true/false, and interview-style questions.
    /// The selected value determines how the question should be presented to users and how responses are
    /// collected.
    /// </remarks>
    public enum QuestionType
    {
        /// <summary>
        /// Represents a question type where only a single answer can be selected from multiple choices.
        /// </summary>
        SINGLE_CHOICE,

        /// <summary>
        /// Represents a question type where the user selects one or more answers from a list of options.
        /// </summary>
        MULTIPLE_CHOICE,

        /// <summary>
        /// Gets or sets the text content associated with this element.
        /// </summary>
        TEXT,

        /// <summary>
        /// Specifies a value that represents a boolean state of true or false.
        /// </summary>
        TRUE_FALSE,

        /// <summary>
        /// Represents an interview or interview-related entity.
        /// </summary>
        INTERVIEW
    }
}