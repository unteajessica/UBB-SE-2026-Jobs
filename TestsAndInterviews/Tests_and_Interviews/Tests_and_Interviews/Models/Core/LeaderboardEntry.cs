namespace Tests_and_Interviews.Models.Core
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a single player's score on the leaderboard.
    /// </summary>
    [Table("LeaderboardEntries")]
    public class LeaderboardEntry
    {
        /// <summary>
        /// Gets or sets the entry's unique identifier.
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the entry's corresponding test id.
        /// </summary>
        [Column("test_id")]
        public int TestId { get; set; }

        /// <summary>
        /// Gets or sets the entry's corresponding user id.
        /// </summary>
        [Column("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the normalized score after taking the test.
        /// </summary>
        [Column("normalized_score", TypeName = "decimal(18,2)")]
        public decimal NormalizedScore { get; set; }

        /// <summary>
        /// Gets or sets the position of the leaderboard entry among the other entries.
        /// </summary>
        [Column("rank_position")]
        public int RankPosition { get; set; }

        /// <summary>
        /// Gets or sets the priority of the current entry in case of a tie.
        /// </summary>
        [Column("tie_break_priority")]
        public int TieBreakPriority { get; set; }

        /// <summary>
        /// Gets or sets the last date when a leaderboard entry's fields were recalculated.
        /// </summary>
        [Column("last_recalculation_at")]
        public DateTime LastRecalculationAt { get; set; }

        /// <summary>
        /// Gets or sets the Test object corresponding to the current Leaderboard Entry.
        /// </summary>
        [ForeignKey("TestId")]
        public Test? Test { get; set; }

        /// <summary>
        /// Gets or sets the User object corresponding to the current LeaderboardEntry.
        /// </summary>
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}