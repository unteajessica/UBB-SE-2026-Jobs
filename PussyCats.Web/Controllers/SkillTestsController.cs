using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Services.SkillTests;

namespace PussyCats.Web.Controllers
{
    [Authorize] // Guard all actions from unauthorized users per requirements
    public class SkillTestsController : Controller
    {
        private readonly ISkillTestService skillTestService;

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public SkillTestsController(ISkillTestService skillTestService)
        {
            this.skillTestService = skillTestService;
        }

        public async Task<IActionResult> Index()
        {
            var tests = await skillTestService.GetTestsForUserAsync(CurrentUserId);
            return View(tests);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Retake(int id)
        {
            bool isEligible = await skillTestService.CanRetakeTestAsync(id);
            if (!isEligible)
            {
                TempData["ErrorMessage"] = "This test is locked. You cannot retake it yet.";
                return RedirectToAction(nameof(Index));
            }

            int randomScore = Random.Shared.Next(0, 101);
            await skillTestService.SubmitRetakeAsync(id, randomScore);
            TempData["SuccessMessage"] = $"Test retaken! New score: {randomScore}%";
            return RedirectToAction(nameof(Index));
        }
    }
}
