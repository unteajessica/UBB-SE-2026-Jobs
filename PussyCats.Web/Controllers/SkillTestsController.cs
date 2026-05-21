using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.SkillTests;

namespace PussyCats.Web.Controllers
{
    //[Authorize] // Guard all actions from unauthorized users per requirements
    public class SkillTestsController : Controller
    {
        private readonly ISkillTestService skillTestService;

        public SkillTestsController(ISkillTestService skillTestService)
        {
            this.skillTestService = skillTestService;
        }

        // 1. GET: /SkillTests
        // Shows all tests for the logged-in user
        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var tests = await skillTestService.GetTestsForUserAsync(userId);
            return View(tests);
        }

        // POST: /SkillTests/Retake
        // Matches the behavior of RetakeCommand/RetakeAsync in your CardViewModel
        [HttpPost]
        [ValidateAntiForgeryToken] // Anti-forgery guard token required per plan rules
        public async Task<IActionResult> Retake(int id)
        {
            // Verify if user can retake first (mirroring your safety checks)
            bool isEligible = await skillTestService.CanRetakeTestAsync(id);
            if (!isEligible)
            {
                TempData["ErrorMessage"] = "This test is locked! You cannot retake it yet.";
                return RedirectToAction(nameof(Index));
            }

            // Generate random score between 0 and 100 exactly like WinUI code
            int randomScore = Random.Shared.Next(0, 101);

            // Submit payload back into the service layer proxy API pipeline
            await skillTestService.SubmitRetakeAsync(id, randomScore);

            TempData["SuccessMessage"] = $"Test retaken successfully! New score achieved: {randomScore}%";
            return RedirectToAction(nameof(Index));
        }
    }
}
