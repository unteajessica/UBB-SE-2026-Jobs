// <copyright file="SlotDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews_API.Dtos
{
    using System;
    using Tests_and_Interviews_API.Models.Enums;

    public record SlotDto
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the recruiter.
        /// </summary>
        public int RecruiterId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the candidate.
        /// </summary>
        public int? CandidateId { get; set; }

        /// <summary>
        /// Gets or sets the start time of the slot.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the slot.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the duration in minutes.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the current status of the slot.
        /// </summary>
        public SlotStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the type of interview.
        /// </summary>
        public string InterviewType { get; set; }

        /// <summary>
        /// Gets the start time formatted as a 24-hour string (HH:mm).
        /// </summary>
        public string FormattedTime => this.StartTime.ToString("HH:mm");

        /// <summary>
        /// Gets the time range in 24-hour format as a string, combining the start and end times.
        /// </summary>
        public string TimeRange => $"{this.StartTime:HH:mm} - {this.EndTime:HH:mm}";
    }
}