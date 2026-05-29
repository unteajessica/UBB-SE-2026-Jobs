// <copyright file="IInterviewSessionService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// Defines the contract for managing interview session operations.
    /// </summary>
    public interface IInterviewSessionService
    {
        /// <summary>
        /// Loads the session and its questions, and marks the session as started.
        /// </summary>
        /// <param name="sessionId">The ID of the interview session.</param>
        /// <returns>A tuple containing the loaded interview session and its questions.</returns>
        Task<(InterviewSession? Session, List<Question> Questions)> StartSessionAsync(int sessionId);

        /// <summary>
        /// Saves the recording path and marks the session as in progress.
        /// </summary>
        /// <param name="session">The current interview session.</param>
        /// <param name="recordingFilePath">The file path of the recorded video.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SubmitRecordingAsync(InterviewSession session, string recordingFilePath);

        /// <summary>
        /// Saves the score and marks the session as completed.
        /// </summary>
        /// <param name="sessionId">The ID of the interview session.</param>
        /// <param name="score">The score given by the interviewer.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SubmitScoreAsync(int sessionId, float score);

        /// <summary>
        /// Loads an interview session by its ID.
        /// </summary>
        /// <param name="sessionId">The ID of the interview session.</param>
        /// <returns>The interview session corresponding to the specified ID.</returns>
        Task<InterviewSession> GetSessionAsync(int sessionId);

        Task<List<InterviewSession>> GetScheduledSessionsAsync();

        Task DeleteSessionAsync(int sessionId);
        Task<List<InterviewSession>> GetSessionsByStatusAsync(string status);
    }
}