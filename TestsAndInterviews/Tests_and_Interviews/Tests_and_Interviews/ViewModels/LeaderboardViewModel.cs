namespace Tests_and_Interviews.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// ViewModel for the leaderboard page and summary leaderboard dialog.
    /// Handles all leaderboard data fetching and pagination logic.
    /// </summary>
    public class LeaderboardViewModel
    {
        private const int PageSize = 10;
        private readonly ILeaderboardService leaderboardService;
        private List<LeaderboardEntry> entries = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardViewModel"/> class.
        /// </summary>
        /// <param name="leaderboardService">Service used to fetch and recalculate leaderboard data.</param>
        public LeaderboardViewModel(ILeaderboardService leaderboardService)
        {
            this.leaderboardService = leaderboardService;
        }

        /// <summary>
        /// Gets the current page number.
        /// </summary>
        public int CurrentPage { get; private set; } = 1;

        /// <summary>
        /// Gets the total number of pages based on the loaded entries and page size.
        /// </summary>
        public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)this.entries.Count / PageSize));

        /// <summary>
        /// Gets a value indicating whether the user can navigate to the previous page.
        /// </summary>
        public bool CanGoPrev => this.CurrentPage > 1;

        /// <summary>
        /// Gets a value indicating whether the user can navigate to the next page.
        /// </summary>
        public bool CanGoNext => this.CurrentPage < this.TotalPages;

        /// <summary>
        /// Loads the full leaderboard for the given test.
        /// </summary>
        /// <param name="testId">The ID of the test to load the leaderboard for.</param>
        /// <returns>A task representing the async operation.</returns>
        public async Task LoadAsync(int testId)
        {
            this.entries = await this.leaderboardService.GetFullLeaderboardAsync(testId);
            this.CurrentPage = 1;
        }

        /// <summary>
        /// Returns the leaderboard entries for the current page.
        /// </summary>
        /// <returns> A list of leaderboard entries. </returns>
        public List<LeaderboardEntry> GetCurrentPageEntries()
        {
            return this.entries
                .Skip((this.CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        /// <summary>
        /// Moves to the previous page if possible.
        /// </summary>
        public void GoToPrevPage()
        {
            if (this.CanGoPrev)
            {
                this.CurrentPage--;
            }
        }

        /// <summary>
        /// Moves to the next page if possible.
        /// </summary>
        public void GoToNextPage()
        {
            if (this.CanGoNext)
            {
                this.CurrentPage++;
            }
        }

        /// <summary>
        /// Loads the top three leaderboard entries for the given test.
        /// </summary>
        /// <param name="testId">The ID of the test.</param>
        /// <returns> A task representing the asynchronous operation. </returns>
        public async Task<List<LeaderboardEntry>> GetTopThreeAsync(int testId)
        {
            return await this.leaderboardService.GetTopThreeAsync(testId);
        }

        /// <summary>
        /// Gets the current user's leaderboard entry for the given test.
        /// </summary>
        /// <param name="userId">The ID of the current user.</param>
        /// <param name="testId">The ID of the test.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<LeaderboardEntry?> GetUserRankingAsync(int userId, int testId)
        {
            return await this.leaderboardService.GetUserRankingAsync(userId, testId);
        }
    }
}