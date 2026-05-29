// <copyright file="IBookingService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// Defines booking-related operations for managing interview slot reservations and session creation.
    /// </summary>
    public interface IBookingService
    {
        /// <summary>
        /// Gets the available slots for a given recruiter at a given date.
        /// </summary>
        /// <param name="recruiterId">Id of the recruiter.</param>
        /// <param name="date">The date for which to retrieve available slots.</param>
        /// <returns>A list of available slots for the specified recruiter and date.</returns>
        public Task<List<Slot>> GetAvailableSlots(int recruiterId, DateTime date);

        /// <summary>
        /// Gets all available slots for a given recruiter, regardless of the date.
        /// </summary>
        /// <param name="recruiterId">Id of the recruiter.</param>
        /// <returns>A list of all available slots for the specified recruiter.</returns>
        public Task<List<Slot>> GetAvailableSlotsByRecruiterId(int recruiterId);

        /// <summary>
        /// Confirms a booking for a candidate by updating the slot's status to occupied and creating a new interview session.
        /// </summary>
        /// <param name="candidateId">Id of the candidate.</param>
        /// <param name="slot">The slot to be booked.</param>
        public Task ConfirmBooking(int candidateId, Slot slot);
    }
}