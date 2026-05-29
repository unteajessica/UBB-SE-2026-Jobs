namespace Tests_and_Interviews_API.Services
{
    using System;
    using System.Collections.Generic;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Provides operations for managing events.
    /// </summary>
    public class EventsService : IEventsService
    {
        private readonly IEventsRepo _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsService"/> class.
        /// </summary>
        /// <param name="repository">The repository used to access event data. Cannot be null.</param>
        public EventsService(IEventsRepo repository)
        {
            this._repository = repository;
        }

        /// <summary>
        /// Adds a new event to the data store.
        /// </summary>
        /// <param name="eventToBeAdded">The event to add. Cannot be null.</param>
        public void AddEventToRepo(Event eventToBeAdded)
        {
            this._repository.AddEventToRepo(eventToBeAdded);
        }

        /// <summary>
        /// Removes the specified event from the data store.
        /// </summary>
        /// <param name="eventToBeRemoved">The event to remove. Cannot be null.</param>
        public void RemoveEventFromRepo(Event eventToBeRemoved)
        {
            this._repository.RemoveEventFromRepo(eventToBeRemoved);
        }

        /// <summary>
        /// Retrieves all current events for the specified company.
        /// </summary>
        /// <param name="loggedInUser">The unique identifier of the logged in company.</param>
        /// <returns>A list of current events for the specified company.</returns>
        public List<Event> GetCurrentEventsFromRepo(int? loggedInUser = null)
        {
            return this._repository.GetCurrentEventsFromRepo(loggedInUser).ToList();
        }

        /// <summary>
        /// Retrieves all past events for the specified company.
        /// </summary>
        /// <param name="loggedInUser">The unique identifier of the logged in company.</param>
        /// <returns>A list of past events for the specified company.</returns>
        public List<Event> GetPastEventsFromRepo(int? loggedInUser = null)
        {
            return this._repository.GetPastEventsFromRepo(loggedInUser).ToList();
        }

        /// <summary>
        /// Updates an existing event in the data store.
        /// </summary>
        /// <param name="id">The unique identifier of the event to update.</param>
        /// <param name="photo">The updated photo of the event.</param>
        /// <param name="title">The updated title of the event.</param>
        /// <param name="description">The updated description of the event.</param>
        /// <param name="start">The updated start date of the event.</param>
        /// <param name="end">The updated end date of the event.</param>
        /// <param name="location">The updated location of the event.</param>
        public void UpdateEventToRepo(int id, string photo, string title, string description, DateTime start, DateTime end, string location)
        {
            this._repository.UpdateEventToRepo(id, photo, title, description, start, end, location);
        }
    }
}