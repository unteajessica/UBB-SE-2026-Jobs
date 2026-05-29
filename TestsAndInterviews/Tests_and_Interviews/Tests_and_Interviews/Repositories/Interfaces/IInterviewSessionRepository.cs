// <copyright file="IInterviewSessionRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Repositories.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// Defines a contract for managing interview sessions, including operations for retrieving, adding, updating, and
    /// deleting sessions.
    /// </summary>
    public interface IInterviewSessionRepository
    {
        /// <summary>
        /// Asynchronously retrieves an interview session by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the interview session to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="InterviewSession"/> corresponding to the specified identifier.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no interview session exists with the specified <paramref name="id"/>.</exception>
        Task<InterviewSession> GetInterviewSessionByIdAsync(int id);

        /// <summary>
        /// Synchronous version of retrieving an interview session by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the interview session to retrieve.</param>
        /// <returns>The <see cref="InterviewSession"/> corresponding to the specified identifier.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no interview session exists with the specified <paramref name="id"/>.</exception>
        InterviewSession GetInterviewSessionById(int id);

        /// <summary>
        /// Asynchronously retrieves a list of all scheduled interview sessions.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="InterviewSession"/> objects representing the scheduled interview sessions. The list is empty if no
        /// sessions are scheduled.</returns>
        Task<List<InterviewSession>> GetScheduledSessionsAsync();

        /// <summary>
        /// Asynchronously retrieves a list of interview sessions that have the specified status.
        /// </summary>
        /// <remarks>This method is asynchronous and should be awaited. Supplying an invalid status may
        /// result in an empty list or unexpected results.</remarks>
        /// <param name="status">The status of the interview sessions to retrieve. Valid values include "Scheduled", "Completed", and
        /// "Cancelled". The comparison is case-sensitive.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of interview sessions
        /// matching the specified status. The list is empty if no sessions are found.</returns>
        Task<List<InterviewSession>> GetSessionsByStatusAsync(string status);

        /// <summary>
        /// Adds the specified interview session to the repository.
        /// </summary>
        /// <remarks>Ensure that the session provided is valid and meets any necessary criteria for
        /// inclusion in the repository.</remarks>
        /// <param name="session">The interview session to add. This parameter cannot be null.</param>
        void Add(InterviewSession session);

        /// <summary>
        /// Asynchronously updates the details of an existing interview session.
        /// </summary>
        /// <remarks>Throws an exception if the provided interview session is null or does not exist in
        /// the system.</remarks>
        /// <param name="updated">The interview session object containing the updated details to apply. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        Task UpdateInterviewSessionAsync(InterviewSession updated);

        /// <summary>
        /// Deletes the specified interview session from the repository.
        /// </summary>
        /// <remarks>Ensure that the session exists before calling this method, as attempting to delete a
        /// non-existent session may result in an error.</remarks>
        /// <param name="session">The interview session to be deleted. This parameter cannot be null.</param>
        void Delete(InterviewSession session);
    }
}
