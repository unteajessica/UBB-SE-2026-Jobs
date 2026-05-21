using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.UserRecommendationService;

namespace PussyCats.Web.Controllers
{
    //[Authorize]
    public class JobBrowserController : Controller
    {
        private readonly IUserRecommendationService recommendationService;

        public JobBrowserController(IUserRecommendationService recommendationService)
        {
            this.recommendationService = recommendationService;
        }

        // 1. Display current job card or empty deck state
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();

            // Just create a fresh, completely empty filter
            var filters = UserMatchmakingFilters.Empty();

            // Call your service proxy
            var jobCard = await recommendationService.GetNextCardAsync(userId, filters);

            // Pass the job card to the view
            return View(jobCard);
        }

        // 2. Action method handler when user clicks "Like" on the web UI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(JobRecommendationResult card)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

            int userId = GetCurrentUserId();
            try
            {
                int matchId = await recommendationService.ApplyLikeAsync(userId, card);

                // Track Undo capabilities using Web HTTP Sessions
                HttpContext.Session.SetInt32("LastMatchId", matchId);
                HttpContext.Session.SetInt32("LastDisplayId", card.DisplayRecommendationId ?? 0);
                HttpContext.Session.SetString("LastAction", "Like");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. Action method handler when user clicks "Dismiss" on the web UI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dismiss(JobRecommendationResult card)
        {
            int userId = GetCurrentUserId();
            int dismissId = await recommendationService.ApplyDismissAsync(userId, card);

            HttpContext.Session.SetInt32("LastDismissId", dismissId);
            HttpContext.Session.SetInt32("LastDisplayId", card.DisplayRecommendationId ?? 0);
            HttpContext.Session.SetString("LastAction", "Dismiss");

            return RedirectToAction(nameof(Index));
        }

        // 4. Action method handler when user clicks "Undo" on the web UI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Undo()
        {
            string? lastAction = HttpContext.Session.GetString("LastAction");
            int? displayId = HttpContext.Session.GetInt32("LastDisplayId");

            if (lastAction == "Like" && HttpContext.Session.GetInt32("LastMatchId") is { } matchId)
            {
                await recommendationService.UndoLikeAsync(matchId, displayId);
            }
            else if (lastAction == "Dismiss" && HttpContext.Session.GetInt32("LastDismissId") is { } dismissId)
            {
                await recommendationService.UndoDismissAsync(dismissId, displayId);
            }

            // Clear session state tracking after single consumption
            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApplyFilters(IFormCollection form)
        {
            int userId = GetCurrentUserId(); // Your user ID resolver

            // 1. Create a clean base filter instance
            var filters = UserMatchmakingFilters.Empty();

            // 2. Manually bind the Location string
            filters.LocationSubstring = form["Location"].ToString() ?? string.Empty;

            // 3. Extract check boxes and push them into the read-only collections
            var submittedEmploymentTypes = form["EmploymentTypes"];
            foreach (var empType in submittedEmploymentTypes)
            {
                if (!string.IsNullOrWhiteSpace(empType))
                {
                    filters.EmploymentTypes.Add(empType);
                }
            }

            var submittedExperienceLevels = form["ExperienceLevels"];
            foreach (var expLevel in submittedExperienceLevels)
            {
                if (!string.IsNullOrWhiteSpace(expLevel))
                {
                    filters.ExperienceLevels.Add(expLevel);
                }
            }

            // 4. Send our cleanly populated filters to the API
            var jobCard = await recommendationService.GetNextCardAsync(userId, filters);

            // Return the view containing the loaded card
            return View("Index", jobCard);
        }

        private int GetCurrentUserId()
        {
            // Helper method matching your app authentication framework infrastructure
            //return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            int mockUserId = 1;
            return mockUserId;
        }
    }
}
