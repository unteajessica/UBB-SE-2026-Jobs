using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.UserRecommendationService;
using PussyCats.Web.Infrastructure;
using System.Security.Claims;

namespace PussyCats.Web.Controllers
{
    [Authorize]
    public class JobBrowserController : Controller
    {
        private readonly IUserRecommendationService recommendationService;

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public JobBrowserController(IUserRecommendationService recommendationService)
        {
            this.recommendationService = recommendationService;
        }

        public async Task<IActionResult> Index()
        {
            var filters = UserMatchmakingFilters.Empty();
            var jobCard = await recommendationService.GetNextCardAsync(CurrentUserId, filters);
            return View(jobCard);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(JobRecommendationResult card)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            try
            {
                int matchId = await recommendationService.ApplyLikeAsync(CurrentUserId, card);
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Dismiss(JobRecommendationResult card)
        {
            int dismissId = await recommendationService.ApplyDismissAsync(CurrentUserId, card);
            HttpContext.Session.SetInt32("LastDismissId", dismissId);
            HttpContext.Session.SetInt32("LastDisplayId", card.DisplayRecommendationId ?? 0);
            HttpContext.Session.SetString("LastAction", "Dismiss");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Undo()
        {
            string? lastAction = HttpContext.Session.GetString("LastAction");
            int? displayId = HttpContext.Session.GetInt32("LastDisplayId");

            if (lastAction == "Like" && HttpContext.Session.GetInt32("LastMatchId") is { } matchId)
                await recommendationService.UndoLikeAsync(matchId, displayId);
            else if (lastAction == "Dismiss" && HttpContext.Session.GetInt32("LastDismissId") is { } dismissId)
                await recommendationService.UndoDismissAsync(dismissId, displayId);

            HttpContext.Session.Remove("LastAction");
            HttpContext.Session.Remove("LastMatchId");
            HttpContext.Session.Remove("LastDismissId");
            HttpContext.Session.Remove("LastDisplayId");

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ResetFilters() => RedirectToAction(nameof(Index));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyFilters(IFormCollection form)
        {
            var filters = UserMatchmakingFilters.Empty();
            filters.LocationSubstring = form["Location"].ToString() ?? string.Empty;

            foreach (var empType in form["EmploymentTypes"])
                if (!string.IsNullOrWhiteSpace(empType))
                    filters.EmploymentTypes.Add(empType);

            foreach (var expLevel in form["ExperienceLevels"])
                if (!string.IsNullOrWhiteSpace(expLevel))
                    filters.ExperienceLevels.Add(expLevel);

            var jobCard = await recommendationService.GetNextCardAsync(CurrentUserId, filters);
            return View("Index", jobCard);
        }
    }
}
