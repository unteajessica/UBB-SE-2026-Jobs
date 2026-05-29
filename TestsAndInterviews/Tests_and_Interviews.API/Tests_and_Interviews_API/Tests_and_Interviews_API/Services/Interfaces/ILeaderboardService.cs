namespace Tests_and_Interviews_API.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Models.Core;

    /// <summary>
    /// Defines operations for managing leaderboard entries.
    /// </summary>
    public interface ILeaderboardService
    {
        /// <summary>
        /// Asynchronously retrieves all leaderboard entries for the specified test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of leaderboard entries for the specified test.</returns>
        Task<List<LeaderboardEntry>> FindByTestIdAsync(int testId);

        /// <summary>
        /// Asynchronously retrieves the top leaderboard entries for the specified test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <param name="limit">The maximum number of entries to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of top leaderboard entries for the specified test.</returns>
        Task<List<LeaderboardEntry>> FindTopByTestIdAsync(int testId, int limit);

        /// <summary>
        /// Asynchronously retrieves the leaderboard entry for the specified user and test.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the leaderboard entry for the specified user and test, or null if not found.</returns>
        Task<LeaderboardEntry?> FindUserEntryAsync(int userId, int testId);

        /// <summary>
        /// Asynchronously deletes all leaderboard entries for the specified test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteByTestIdAsync(int testId);

        /// <summary>
        /// Asynchronously saves a list of leaderboard entries to the data store.
        /// </summary>
        /// <param name="entries">The list of leaderboard entries to save. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SaveRangeAsync(List<LeaderboardEntry> entries);

        /// <summary>
        /// Recalculates the leaderboard for the specified test by fetching valid attempts,
        /// clearing existing entries, and saving newly ranked entries.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RecalculateAsync(int testId);
    }
}