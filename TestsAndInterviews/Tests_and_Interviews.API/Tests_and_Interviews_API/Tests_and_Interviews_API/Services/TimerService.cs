// <copyright file="TimerService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews_API.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Helpers;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Models.Enums;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// TimerService class manages the timing of test attempts, ensuring that tests are submitted within a specified duration.
    /// It provides methods to start timers for test attempts, check for expiration, and expire tests when necessary.
    /// This service is crucial for enforcing time limits on tests and maintaining the integrity of the testing process.
    /// </summary>
    public class TimerService : ITimerService
    {
        private static readonly ConcurrentDictionary<int, DateTime> Timers = new ();
        private static readonly TimeSpan TestDuration = TimeSpan.FromMinutes(TestConstants.TestDurationInMinutes);
        private readonly ITestAttemptRepository testAttemptRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerService"/> class with the specified TestAttemptRepository.
        /// </summary>
        /// <param name="testAttemptRepository">The repository used to manage test attempts in the database.</param>
        public TimerService(ITestAttemptRepository testAttemptRepository)
        {
            this.testAttemptRepository = testAttemptRepository;
        }

        /// <summary>
        /// Starts a timer for a given test attempt ID by recording the current UTC time.
        /// This method is typically called when a user begins a test attempt, allowing the service to track
        /// how long the attempt has been active and determine if it has exceeded the allowed duration.
        /// </summary>
        /// <param name="attemptId">The unique identifier of the test attempt for which the timer is being started.</param>
        public void StartTimer(int attemptId)
        {
            Timers[attemptId] = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the timer for a given test attempt ID has expired by comparing the current UTC time with the recorded start time.
        /// </summary>
        /// <param name="attemptId">The unique identifier of the test attempt to check for expiration.</param>
        /// <returns>True if the timer has expired; otherwise, false.</returns>
        public bool CheckExpiration(int attemptId)
        {
            if (!Timers.TryGetValue(attemptId, out DateTime startTime))
            {
                return false;
            }

            return DateTime.UtcNow - startTime > TestDuration;
        }

        /// <summary>
        /// Gets a list of test attempt IDs that have expired by iterating through the timers and checking if any
        /// have exceeded the allowed duration.
        /// </summary>
        /// <returns>A list of test attempt IDs that have expired.</returns>
        public List<int> GetExpiredAttemptIds()
        {
            var expired = new List<int>();
            foreach (var timerEntry in Timers)
            {
                if (DateTime.UtcNow - timerEntry.Value > TestDuration)
                {
                    expired.Add(timerEntry.Key);
                }
            }

            return expired;
        }

        /// <summary>
        /// Expires a test attempt by updating its status to "SUBMITTED" and setting the completion time to the current UTC time.
        /// </summary>
        /// <param name="attemptId">The unique identifier of the test attempt to expire.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ExpireTestAsync(int attemptId)
        {
            var expiredAttempt = new TestAttempt
            {
                Id = attemptId,
                Status = TestStatus.COMPLETED.ToString(),
                CompletedAt = DateTime.UtcNow,
            };

            await this.testAttemptRepository.UpdateAsync(expiredAttempt);
            Timers.TryRemove(attemptId, out _);
        }
    }
}