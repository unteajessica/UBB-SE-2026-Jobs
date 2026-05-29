namespace Tests_and_Interviews_API.Dtos
{
    using System;

    /// <summary>
    /// Represents a single player's score on the leaderboard.
    /// </summary>
    public class LeaderboardEntryDto
    {
        /// <summary>
        /// Gets or sets the entry's unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the entry's corresponding test id.
        /// </summary>
        public int TestId { get; set; }

        /// <summary>
        /// Gets or sets the entry's corresponding user id.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the normalized score after taking the test.
        /// </summary>
        public decimal NormalizedScore { get; set; }

        /// <summary>
        /// Gets or sets the position of the leaderboard entry among the other entries.
        /// </summary>
        public int RankPosition { get; set; }

        /// <summary>
        /// Gets or sets the priority of the current entry in case of a tie.
        /// </summary>
        public int TieBreakPriority { get; set; }

        /// <summary>
        /// Gets or sets the last date when a leaderboard entry's fields were recalculated.
        /// </summary>
        public DateTime LastRecalculationAt { get; set; }
    }
}