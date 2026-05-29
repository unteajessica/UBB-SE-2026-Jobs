namespace Tests_and_Interviews_API.Services.Interfaces
{
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Models;

    /// <summary>
    /// Service layer for Slot CRUD operations.
    /// Sits between the controller and the repository.
    /// </summary>
    public interface ISlotService
    {
        /// <summary>
        /// Asynchronously retrieves the list of available slots for a specified recruiter on a given date.
        /// </summary>
        /// <param name="recruiterId">The unique identifier of the recruiter whose slots are to be retrieved.</param>
        /// <param name="date">The date for which to retrieve available slots.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of slots available for
        /// the specified recruiter on the given date. The list is empty if no slots are available.</returns>
        public Task<List<Slot>> GetSlotsAsync(int recruiterId, DateTime date);

        public Task<List<SlotDto>> GetAvailableSlotsForDateAsync(DateTime date);

        /// <summary>
        /// Asynchronously retrieves all available slots associated with the specified recruiter.
        /// </summary>
        /// <param name="recruiterId">The unique identifier of the recruiter whose slots are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of slots for the
        /// specified recruiter. The list is empty if no slots are found.</returns>
        public Task<List<Slot>> GetAllSlotsAsync(int recruiterId);

        /// <summary>
        /// Asynchronously retrieves a slot with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the slot to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the slot with the specified
        /// identifier.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a slot with the specified identifier does not exist.</exception>
        public Task<Slot> GetSlotByIdAsync(int id);

        /// <summary>
        /// Asynchronously adds a new slot to the data store.
        /// </summary>
        /// <param name="slot">The slot to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added slot.</returns
        public Task<Slot> AddSlotAsync(Slot slot);

        /// <summary>
        /// Updates the slot with the specified identifier using the provided slot data asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the slot to update.</param>
        /// <param name="slot">The slot data to use for the update. The <see cref="Slot.Id"/> property is ignored and replaced with the
        /// value of <paramref name="id"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="Slot"/>
        /// instance.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a slot with the specified <paramref name="id"/> does not exist.</exception>
        public Task<Slot> UpdateSlotAsync(int id, Slot slot);

        /// <summary>
        /// Asynchronously deletes the slot with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the slot to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the slot was
        /// successfully deleted.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a slot with the specified <paramref name="id"/> does not exist.</exception
        public Task<bool> DeleteSlotAsync(int id);

        Task<List<SlotDto>> LoadRecruiterVisibleSlotsAsync(int recruiterId, DateTime date);
        Task CreateRecruiterSlotAsync(SlotDto baseSlot, int duration);
        Task UpdateRecruiterSlotAsync(SlotDto initialSlot, DateTime startTime, int duration);
    }
}
