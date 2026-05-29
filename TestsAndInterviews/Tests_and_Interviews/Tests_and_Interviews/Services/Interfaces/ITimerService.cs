namespace Tests_and_Interviews.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// ITimerService interface provides methods to manage timers for test attempts, including starting timers, checking for expiration, and expiring test attempts when necessary.
    /// </summary>
    public interface ITimerService
    {
        /// <summary>
        /// Starts a timer for a given test attempt ID by recording the current UTC time.
        /// This method is typically called when a user begins a test attempt, allowing the service to track
        /// how long the attempt has been active and determine if it has exceeded the allowed duration.
        /// </summary>
        /// <param name="attemptId">The unique identifier of the test attempt for which the timer is being started.</param>
        void StartTimer(int attemptId);

        /// <summary>
        /// Checks if the timer for a given test attempt ID has expired by comparing the current UTC time with the recorded start time.
        /// </summary>
        /// <param name="attemptId">The unique identifier of the test attempt to check for expiration.</param>
        /// <returns>True if the timer has expired; otherwise, false.</returns>
        bool CheckExpiration(int attemptId);

        /// <summary>
        /// Gets a list of test attempt IDs that have expired by iterating through the timers and checking if any
        /// have exceeded the allowed duration.
        /// </summary>
        /// <returns>A list of test attempt IDs that have expired.</returns>
        List<int> GetExpiredAttemptIds();

        /// <summary>
        /// Expires a test attempt by updating its status to "SUBMITTED" and setting the completion time to the current UTC time.
        /// </summary>
        /// <param name="attemptId">The unique identifier of the test attempt to expire.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ExpireTestAsync(int attemptId);
    }
}
