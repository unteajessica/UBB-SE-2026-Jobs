using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Services.UserProfileService;

namespace PussyCats.Web.Controllers
{
    //[Authorize] // Fulfills security constraints
    public class ProfileController : Controller
    {
        private readonly IUserProfileService _profileService;
        private readonly ICompletenessService _completenessService;

        public ProfileController(IUserProfileService profileService, ICompletenessService completenessService)
        {
            _profileService = profileService;
            _completenessService = completenessService;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            int mockUserId = 1; // Swap with true identity contexts later

            var user = await _profileService.GetProfileAsync(mockUserId);
            if (user == null) return NotFound();

            // Calculate support metadata exactly like your WinUI viewmodel logic
            ViewBag.CompletenessPercentage = _completenessService.CalculateCompleteness(user);
            ViewBag.NextFieldPrompt = _completenessService.GetNextEmptyFieldPrompt(user);
            ViewBag.TotalXp = await _profileService.RecalculateLevelAsync(user);

            return View(user);
        }

        // POST: /Profile/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus()
        {
            int mockUserId = 1;
            var user = await _profileService.GetProfileAsync(mockUserId);
            if (user == null) return NotFound();

            await _profileService.UpdateAccountStatusAsync(mockUserId, !user.ActiveAccount);
            return RedirectToAction(nameof(Index));
        }
    }
}
