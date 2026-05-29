using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tests_and_Interviews.Web.Clients;
using Tests_and_Interviews.Web.Models;

namespace Tests_and_Interviews.Web.Controllers
{
    [Authorize(Policy = "AuthenticatedUser")]
    public class LeaderboardController : Controller
    {
        private readonly LeaderboardApiClient leaderboardApiClient;
        private readonly UsersApiClient usersApiClient;

        public LeaderboardController(
            LeaderboardApiClient leaderboardApiClient,
            UsersApiClient usersApiClient)
        {
            this.leaderboardApiClient = leaderboardApiClient;
            this.usersApiClient = usersApiClient;
        }

        public async Task<IActionResult> Index(int testId)
        {
            if (testId <= 0)
            {
                return this.BadRequest();
            }

            var currentUser = await this.usersApiClient.GetCurrentUser();

            if (currentUser == null)
            {
                return this.Challenge();
            }

            // Recalculate the leaderboard to ensure rankings are up-to-date
            await this.leaderboardApiClient.RecalculateLeaderboardAsync(testId);

            var entries = await this.leaderboardApiClient.GetByTestId(testId);
            var currentUserEntry = await this.leaderboardApiClient.GetUserEntry(testId, currentUser.Id);

            LeaderboardViewModel viewModel = new LeaderboardViewModel
            {
                TestId = testId,
                Entries = entries.OrderBy(entry => entry.RankPosition).ToList(),
                CurrentUserEntry = currentUserEntry,
            };

            return this.View(viewModel);
        }
    }
}