// <copyright file="ISlotRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models;

    /// <summary>
    /// Defines methods for managing Slot entities, including retrieval, creation, update, and deletion operations, with
    /// both synchronous and asynchronous support.
    /// </summary>
    /// <remarks>Intended for use in repository patterns to abstract data access logic for Slot entities.</remarks>
    public interface ISlotRepository
    {
        /// <summary>
        /// Asynchronously retrieves available slots for a recruiter on a specified date.
        /// </summary>
        /// <param name="recruiterId">The unique identifier of the recruiter.</param>
        /// <param name="date">The date for which to retrieve slots.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of available slots.</returns>
        public Task<List<Slot>> GetSlotsAsync(int recruiterId, DateTime date);

        /// <summary>
        /// Asynchronously retrieves all slots associated with the specified recruiter.
        /// </summary>
        /// <param name="recruiterId">The unique identifier of the recruiter.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of slots.</returns>
        public Task<List<Slot>> GetAllSlotsAsync(int recruiterId);

        /// <summary>
        /// Asynchronously retrieves a slot by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the slot.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the slot if found; otherwise, null.</returns>
        public Task<Slot?> GetByIdAsync(int id);

        /// <summary>
        /// Asynchronously adds the specified slot.
        /// </summary>
        /// <param name="slot">The slot to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task AddAsync(Slot slot);

        /// <summary>
        /// Asynchronously updates the specified slot.
        /// </summary>
        /// <param name="slot">The slot to update.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        public Task UpdateAsync(Slot slot);

        /// <summary>
        /// Asynchronously deletes the slot with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the slot to delete.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        public Task DeleteAsync(int id);

        /// <summary>
        /// Retrieves available slots for a specified recruiter on a given date.
        /// </summary>
        /// <param name="recruiterId">The unique identifier of the recruiter.</param>
        /// <param name="date">The date for which to retrieve slots.</param>
        /// <returns>A list of available slots for the recruiter on the specified date.</returns>
        public List<Slot> GetSlots(int recruiterId, DateTime date);

        /// <summary>
        /// Retrieves all slots associated with the specified recruiter.
        /// </summary>
        /// <param name="recruiterId">The unique identifier of the recruiter.</param>
        /// <returns>A list of slots belonging to the recruiter.</returns>
        public List<Slot> GetAllSlots(int recruiterId);

        /// <summary>
        /// Retrieves a slot with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the slot.</param>
        /// <returns>The slot with the specified identifier, or null if not found.</returns>
        public Slot? GetById(int id);

        /// <summary>
        /// Adds the specified slot to the collection.
        /// </summary>
        /// <param name="slot">The slot to add.</param>
        public void Add(Slot slot);

        /// <summary>
        /// Updates the specified slot with new values.
        /// </summary>
        /// <param name="slot">The slot to update.</param>
        public void Update(Slot slot);

        /// <summary>
        /// Deletes the slot with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the slot to delete.</param>
        public void Delete(int id);
    }
}