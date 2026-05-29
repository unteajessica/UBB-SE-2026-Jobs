namespace Tests_and_Interviews_API.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Tests_and_Interviews_API.Models;

    /// <summary>
    /// Defines operations for managing events.
    /// </summary>
    public interface IEventsService
    {
        /// <summary>
        /// Adds a new event to the data store.
        /// </summary>
        /// <param name="eventToBeAdded">The event to add. Cannot be null.</param>
        void AddEventToRepo(Event eventToBeAdded);

        /// <summary>
        /// Removes the specified event from the data store.
        /// </summary>
        /// <param name="eventToBeRemoved">The event to remove. Cannot be null.</param>
        void RemoveEventFromRepo(Event eventToBeRemoved);

        /// <summary>
        /// Retrieves all current events for the specified company.
        /// </summary>
        /// <param name="loggedInUser">The unique identifier of the logged in company.</param>
        /// <returns>A list of current events for the specified company.</returns>
        List<Event> GetCurrentEventsFromRepo(int? loggedInUser=null);

        /// <summary>
        /// Retrieves all past events for the specified company.
        /// </summary>
        /// <param name="loggedInUser">The unique identifier of the logged in company.</param>
        /// <returns>A list of past events for the specified company.</returns>
        List<Event> GetPastEventsFromRepo(int? loggedInUser=null);

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
        void UpdateEventToRepo(int id, string photo, string title, string description, DateTime start, DateTime end, string location);
    }
}