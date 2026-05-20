using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Library.Services.CompletenessService;

namespace PussyCats.Web.Controllers
{
    //[Authorize] // Fulfills security constraints
    public class UserProfileController : Controller
    {
        private readonly IUserProfileService userProfileService;
        private readonly ICompletenessService completenessService;

        public UserProfileController(IUserProfileService profileService, ICompletenessService completenessService)
        {
            userProfileService = profileService;
            this.completenessService = completenessService;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            int mockUserId = 1; // Swap with true identity contexts later

            var user = await userProfileService.GetProfileAsync(mockUserId);
            if (user == null) return NotFound();

            // Calculate support metadata exactly like your WinUI viewmodel logic
            ViewBag.CompletenessPercentage = completenessService.CalculateCompleteness(user);
            ViewBag.NextFieldPrompt = completenessService.GetNextEmptyFieldPrompt(user);
            ViewBag.TotalXp = await userProfileService.RecalculateLevelAsync(user);

            return View(user);
        }

        // POST: /Profile/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus()
        {
            int mockUserId = 1;
            var user = await userProfileService.GetProfileAsync(mockUserId);
            if (user == null) return NotFound();

            await userProfileService.UpdateAccountStatusAsync(mockUserId, !user.ActiveAccount);
            return RedirectToAction(nameof(Index));
        }
    }
}
