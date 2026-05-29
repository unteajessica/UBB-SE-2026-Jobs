// <copyright file="InterviewSession.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Models.Core
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents an interview session between for a given position.
    /// </summary>
    [Table("InterviewSessions")]
    public class InterviewSession
    {
        /// <summary>
        /// Gets or sets the unique identifier for the interview session.
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the session identifier. Alias for <see cref="Id"/>, not mapped to a database column.
        /// </summary>
        [NotMapped]
        public int SessionId
        {
            get => this.Id;
            set => this.Id = value;
        }

        /// <summary>
        /// Gets or sets the identifier of the position this session is associated with.
        /// </summary>
        [Column("position_id")]
        public int PositionId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the external user (candidate) participating in the session. Can be null if not yet assigned.
        /// </summary>
        [Column("external_user_id")]
        public int? ExternalUserId { get; set; }
        public User? User { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the interviewer conducting the session.
        /// </summary>
        [Column("interviewer_id")]
        public int InterviewerId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the session is scheduled to begin.
        /// </summary>
        [Column("date_start")]
        public DateTime DateStart { get; set; }

        /// <summary>
        /// Gets or sets the URL or path to the video recording of the session. Can be null if no recording exists.
        /// </summary>
        [Column("video")]
        [MaxLength(200)]
        public string? Video { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current status of the session (e.g., scheduled, completed, cancelled). Can be null if not yet set.
        /// </summary>
        [Column("status")]
        [MaxLength(200)]
        public string? Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the score awarded to the candidate following the session. Can be null if not yet evaluated.
        /// </summary>
        [Column("score", TypeName = "decimal(18,2)")]
        public decimal? Score { get; set; }

    }
}