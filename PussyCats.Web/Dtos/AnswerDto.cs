// <copyright file="AnswerDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PussyCats.Web.Dtos
{
    /// <summary>
    /// Represents an answer to a question in a test attempt.
    /// Used to transfer answer data between layers when submitting or retrieving test results.
    /// </summary>
    public class AnswerDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the question being answered.
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// Gets or sets the answer value provided by the user.
        /// For single/multiple choice questions this is the selected option index or indices;
        /// for text questions it is the free-text response.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier of the attempt in which this answer was given.
        /// </summary>
        public int AttemptId { get; set; }

        /// <summary>
        /// Optional nested question payload. Some endpoints return question details inline with the answer.
        /// Including this allows tests to round-trip an Answer containing a Question via the DTOs.
        /// </summary>
        public QuestionDto? Question { get; set; }
    }
}
