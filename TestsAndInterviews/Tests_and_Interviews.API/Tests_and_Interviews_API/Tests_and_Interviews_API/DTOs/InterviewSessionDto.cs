namespace Tests_and_Interviews_API.Dtos
{
    using System;

    public class InterviewSessionDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the position.
        /// </summary>
        public int PositionId { get; set; }

        /// <summary>
        /// Gets or sets the external user identifier associated with this entity.
        /// </summary>
        public int? ExternalUserId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the interviewer.
        /// </summary>
        public int InterviewerId { get; set; }

        /// <summary>
        /// Gets or sets the start date for the associated interview session.
        /// </summary>
        public DateTime DateStart { get; set; }

        /// <summary>
        /// Gets or sets the URL of the associated video content.
        /// </summary>
        public string? Video { get; set; }

        /// <summary>
        /// Gets or sets the current status of the interview session.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the score value associated to the interview.
        /// </summary>
        public decimal? Score { get; set; }
    }
}
