namespace Tests_and_Interviews.Web.Models
{
    using System.Collections.Generic;
    using Tests_and_Interviews.Web.Dtos;

    /// <summary>
    /// View model used to transport test data and capture candidate answers 
    /// for the test-taking interface.
    /// </summary>
    public class TakeTestViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the test being taken.
        /// </summary>
        public int TestId { get; set; }

        /// <summary>
        /// Gets or sets the title of the test.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of questions to be displayed to the candidate.
        /// </summary>
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

        /// <summary>
        /// Gets or sets the dictionary binding the candidate's answers from the HTML form.
        /// </summary>
        public Dictionary<int, List<string>> Answers { get; set; } = new Dictionary<int, List<string>>();
    }
}