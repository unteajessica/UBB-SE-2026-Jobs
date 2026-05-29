// <copyright file="ISlotService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Dtos;

    /// <summary>
    /// Provides operations for managing recruiter slots.
    /// </summary>
    public interface ISlotService
    {
        /// <summary>
        /// Asynchronously retrieves a list of slots for the specified recruiter and date, including both
        /// existing free and occupied slots and free 30-minutes slots between 8:00 and 18:00.
        /// </summary>
        /// <param name="recruitedId">The unique identifier of the recruiter.</param>
        /// <param name="date">The date for which to load the slots.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of slot DTOs for the specified
        /// recruiter and date.</returns>
        public Task<List<SlotDto>> LoadRecruiterVisibleSlotsAsync(int recruitedId, DateTime date);

        /// <summary>
        /// Creates a new recruiter slot with the specified start time and duration.
        /// </summary>
        /// <param name="baseSlot">The base slot containing the start time.</param>
        /// <param name="duration">The duration of the slot in minutes.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task CreateRecruiterSlotAsync(SlotDto baseSlot, int duration);

        /// <summary>
        /// Asynchronously deletes a recruiter slot with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the recruiter slot to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        public Task DeleteRecruiterSlotAsync(int id);

        /// <summary>
        /// Updates a recruiter's slot with new start time and duration asynchronously.
        /// </summary>
        /// <param name="initialSlot">The initial slot to update.</param>
        /// <param name="startTime">The new start time for the slot.</param>
        /// <param name="duration">The new duration of the slot in minutes.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        /// <exception cref="Exception">Thrown when the start time is outside the allowed hours of 8 to 18.</exception>
        public Task UpdateRecruiterSlotAsync(SlotDto initialSlot, DateTime startTime, int duration);
    }
}