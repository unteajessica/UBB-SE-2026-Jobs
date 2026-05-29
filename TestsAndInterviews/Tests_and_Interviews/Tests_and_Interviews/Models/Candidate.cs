// <copyright file="Candidate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a candidate in the recruitment system, including application status, assigned recruiter, matched
    /// company, and available booking slots.
    /// </summary>
    [NotMapped]
    public class Candidate
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the recruiter assigned to this entity.
        /// </summary>
        public int AssignedRecruiterId { get; set; }

        /// <summary>
        /// Gets or sets the current status of the application.
        /// </summary>
        public string ApplicationStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the company that was matched during processing.
        /// </summary>
        public string MatchedCompany { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of available slots.
        /// </summary>
        [NotMapped]
        public List<Slot> AvailableSlots { get; set; } = new List<Slot>();

        // public List<Slot> BrowseAvailableDates()
        // {
        //     return this.AvailableSlots;
        // }
        // seems useless, not deleting in case something comes up TODO: delete before presenting :)

        /// <summary>
        /// Retrieves a list of available slots for a specific date.
        /// </summary>
        /// <param name="date"> The date for which we want to find available slots.</param>
        /// <returns> A list of <see cref="Slot"/> objects representing the available slots at the specified date.</returns>
        public List<Slot> ViewAvailableSlots(DateTime date)
        {
            return this.AvailableSlots.FindAll(s => s.StartTime.Date == date.Date);
        }

        /// <summary>
        /// Locks a certain slot for the candidate.
        /// </summary>
        /// <param name="slot"> The slot we want to lock.</param>
        /// <exception cref="Exception"> Thrown if slot isnt available.</exception>
        public void BookSlot(Slot slot)
        {
            if (!slot.IsAvailable)
            {
                throw new Exception("Slot is not available");
            }

            slot.Lock(this.Id);
        }
    }
}
