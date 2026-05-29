namespace Tests_and_Interviews_API.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Provides operations for managing leaderboard entries.
    /// </summary>
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ILeaderboardRepository _repository;
        private readonly ITestAttemptRepository _testAttemptRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardService"/> class.
        /// </summary>
        /// <param name="repository">The repository used to access leaderboard data. Cannot be null.</param>
        /// <param name="testAttemptRepository">The repository used to access test attempt data. Cannot be null.</param>
        public LeaderboardService(ILeaderboardRepository repository, ITestAttemptRepository testAttemptRepository)
        {
            this._repository = repository;
            this._testAttemptRepository = testAttemptRepository;
        }

        /// <summary>
        /// Asynchronously retrieves all leaderboard entries for the specified test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of leaderboard entries for the specified test.</returns>
        public async Task<List<LeaderboardEntry>> FindByTestIdAsync(int testId)
        {
            return await this._repository.FindByTestIdAsync(testId);
        }

        /// <summary>
        /// Asynchronously retrieves the top leaderboard entries for the specified test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <param name="limit">The maximum number of entries to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of top leaderboard entries for the specified test.</returns>
        public async Task<List<LeaderboardEntry>> FindTopByTestIdAsync(int testId, int limit)
        {
            return await this._repository.FindTopByTestIdAsync(testId, limit);
        }

        /// <summary>
        /// Asynchronously retrieves the leaderboard entry for the specified user and test.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the leaderboard entry for the specified user and test, or null if not found.</returns>
        public async Task<LeaderboardEntry?> FindUserEntryAsync(int userId, int testId)
        {
            return await this._repository.FindUserEntryAsync(userId, testId);
        }

        /// <summary>
        /// Asynchronously deletes all leaderboard entries for the specified test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task DeleteByTestIdAsync(int testId)
        {
            await this._repository.DeleteByTestIdAsync(testId);
        }

        /// <summary>
        /// Asynchronously saves a list of leaderboard entries to the data store.
        /// </summary>
        /// <param name="entries">The list of leaderboard entries to save. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SaveRangeAsync(List<LeaderboardEntry> entries)
        {
            await this._repository.SaveRangeAsync(entries);
        }

        /// <summary>
        /// Recalculates the leaderboard for the specified test by fetching valid attempts,
        /// clearing existing entries, and saving newly ranked entries.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RecalculateAsync(int testId)
        {
            List<TestAttempt> attempts = await this._testAttemptRepository.FindValidAttemptsByTestIdAsync(testId);

            await this._repository.DeleteByTestIdAsync(testId);

            var entries = new List<LeaderboardEntry>();
            for (int i = 0; i < attempts.Count; i++)
            {
                var attempt = attempts[i];
                entries.Add(new LeaderboardEntry
                {
                    TestId = attempt.TestId,
                    UserId = attempt.ExternalUserId!.Value,
                    NormalizedScore = attempt.PercentageScore!.Value,
                    RankPosition = i + 1,
                    TieBreakPriority = i + 1,
                    LastRecalculationAt = DateTime.UtcNow,
                });
            }

            if (entries.Count > 0)
                await this._repository.SaveRangeAsync(entries);
        }
    }
}