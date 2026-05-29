namespace Tests_and_Interviews.Web.Dtos
{
    using System;
using System.Collections.Generic;

    /// <summary>
    /// Represents an attempt by a user to complete a test.
    /// </summary>
    public class TestAttemptDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test attempt.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the test.
        /// </summary>
        public int TestId { get; set; }

        /// <summary>
        /// Gets or sets the external user identifier associated with this attempt.
        /// </summary>
        public int? ExternalUserId { get; set; }

        /// <summary>
        /// Gets or sets the score value associated with this attempt.
        /// </summary>
        public decimal? Score { get; set; }

        /// <summary>
        /// Gets or sets the current status of the test attempt.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the attempt was started.
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the attempt was completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets the file path to the answers file associated with this attempt.
        /// </summary>
        public string AnswersFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the attempt has been validated.
        /// </summary>
        public bool IsValidated { get; set; }

        /// <summary>
        /// Gets or sets the percentage score associated with this attempt.
        /// </summary>
        public decimal? PercentageScore { get; set; }

        /// <summary>
        /// Gets or sets the reason provided for rejecting the attempt.
        /// </summary>
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the attempt was rejected.
        /// </summary>
        public DateTime? RejectedAt { get; set; }

        /// <summary>
        /// Answers submitted for this attempt (if available).
        /// </summary>
        public List<AnswerDto>? Answers { get; set; }
    }
}