namespace Tests_and_Interviews.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models.Core;

    /// <summary>
    /// Provides business logic for calculating and retrieving leaderboard rankings.
    /// </summary>
    public interface ILeaderboardService
    {
        /// <summary>
        /// Recalculates the rankings for a specific test by processing all valid attempts.
        /// </summary>
        /// <param name="testId">The unique identifier of the test to rank.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RecalculateLeaderboardAsync(int testId);

        /// <summary>
        /// Retrieves the top three leaderboard entries for a specific test after recalculating current scores.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task representing the asynchronous operation, containing the top three entries.</returns>
        Task<List<LeaderboardEntry>> GetTopThreeAsync(int testId);

        /// <summary>
        /// Retrieves the specific ranking information for a user on a given test.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task representing the asynchronous operation, containing the user's entry or null if not found.</returns>
        Task<LeaderboardEntry?> GetUserRankingAsync(int userId, int testId);

        /// <summary>
        /// Retrieves the entire leaderboard for a specific test after performing a recalculation.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <returns>A task representing the asynchronous operation, containing all leaderboard entries.</returns>
        Task<List<LeaderboardEntry>> GetFullLeaderboardAsync(int testId);
    }
}
