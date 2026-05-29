namespace Tests_and_Interviews_API.Services.Interfaces
{
    using Tests_and_Interviews_API.Models.Core;

    /// <summary>
    /// Service layer for InterviewSession CRUD operations.
    /// Sits between the controller and the repository.
    /// </summary>
    public interface IInterviewSessionService
    {
        /// <summary>
        /// Asynchronously retrieves a list of all scheduled interview sessions.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="InterviewSession"/> objects representing the scheduled sessions. The list is empty if no sessions are
        /// scheduled.</returns>
        public Task<List<InterviewSession>> GetScheduledSessionsAsync();

        /// <summary>
        /// Asynchronously retrieves a list of interview sessions that match the specified status.
        /// </summary>
        /// <param name="status">The status value to filter interview sessions by. This parameter is case-sensitive and must not be null or
        /// empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of interview sessions
        /// with the specified status. The list is empty if no sessions match the status.</returns>
        public Task<List<InterviewSession>> GetInterviewsByStatusAsync(string status);

        /// <summary>
        /// Asynchronously retrieves the interview session with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the interview session to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the interview session associated
        /// with the specified identifier.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if an interview session with the specified identifier does not exist.</exception>
        public Task<InterviewSession> GetInterviewByIdAsync(int id);

        /// <summary>
        /// Adds a new interview session to the data store asynchronously.
        /// </summary>
        /// <param name="session">The interview session to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added interview session.</returns>
        public Task<InterviewSession> AddInterviewAsync(InterviewSession session);

        /// <summary>
        /// Updates an existing interview session with new values asynchronously.
        /// </summary>
        /// <param name="id">The unique identifier of the interview session to update.</param>
        /// <param name="session">The updated interview session data. The session's Id property is ignored and replaced with the value of the
        /// specified id.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated interview session.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if an interview session with the specified id does not exist.</exception>
        public Task<InterviewSession> UpdateInterviewAsync(int id, InterviewSession session);

        /// <summary>
        /// Asynchronously deletes the interview session with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the interview session to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the
        /// interview session was successfully deleted.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if an interview session with the specified <paramref name="id"/> does not exist.</exception>

        public Task<bool> DeleteInterviewAsync(int id);

        /// <summary>
        /// Uploads a video file for the specified interview session and returns the updated session along with the
        /// public URL to access the uploaded video.
        /// </summary>
        /// <remarks>The uploaded video is saved to the server's storage and the interview session is
        /// updated to reference the new video. The returned URL can be used by clients to access the video
        /// directly.</remarks>
        /// <param name="sessionId">The unique identifier of the interview session to which the video will be uploaded.</param>
        /// <param name="file">The video file to upload. Must be a valid, non-null file provided by the client.</param>
        /// <param name="requestScheme">The HTTP request scheme (such as "http" or "https") used to construct the public video URL.</param>
        /// <param name="requestHost">The HTTP request host (domain and port) used to construct the public video URL.</param>
        /// <returns>A tuple containing the updated interview session and the public URL for accessing the uploaded video.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if an interview session with the specified sessionId does not exist.</exception>
        public Task<InterviewSession> UploadVideoAsync(int sessionId, IFormFile video);

        /// <summary>
        /// Asynchronously retrieves the contents of the specified video file as a byte array along with its MIME
        /// content type.
        /// </summary>
        /// <param name="fileName">The name of the video file to retrieve. This value must correspond to an existing file in the configured
        /// storage location.</param>
        /// <returns>A tuple containing the video file's contents as a byte array and the MIME content type string. The content
        /// type is always "video/mp4".</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a video file with the specified name does not exist in the storage location.</exception>
        public Task<(byte[], string)> GetVideoAsync(string videoName);
    }
}
