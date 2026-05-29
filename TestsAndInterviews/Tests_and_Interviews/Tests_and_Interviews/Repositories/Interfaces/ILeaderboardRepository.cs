namespace Tests_and_Interviews.Repositories.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// Defines the data access contract for leaderboard operations.
    /// </summary>
    public interface ILeaderboardRepository
    {
        /// <summary>
        /// Retrieves all leaderboard entries associated with a specific test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of leaderboard entries.</returns>
        public Task<List<LeaderboardEntry>> FindByTestIdAsync(int testId);

        /// <summary>
        /// Retrieves the top-ranking leaderboard entries for a specific test, limited by a specific count.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <param name="limit">The maximum number of entries to return.</param>
        /// <returns>A task representing the asynchronous operation, containing the top leaderboard entries.</returns>
        public Task<List<LeaderboardEntry>> FindTopByTestIdAsync(int testId, int limit);

        /// <summary>
        /// Retrieves the specific leaderboard entry for a single user on a specific test.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task representing the asynchronous operation, containing the entry if found; otherwise, null.</returns>
        public Task<LeaderboardEntry?> FindUserEntryAsync(int userId, int testId);

        /// <summary>
        /// Deletes all leaderboard entries associated with a specific test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test to clear.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task DeleteByTestIdAsync(int testId);

        /// <summary>
        /// Saves a collection of leaderboard entries to the database within a single transaction.
        /// </summary>
        /// <param name="entries">The list of entries to persist.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SaveRangeAsync(List<LeaderboardEntry> entries);
    }
}
