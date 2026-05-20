using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.SkillTests;

namespace PussyCats.Web.Controllers
{
    //[Authorize] // Guard all actions from unauthorized users per requirements
    public class SkillTestsController : Controller
    {
        private readonly ISkillTestService _skillTestService;

        public SkillTestsController(ISkillTestService skillTestService)
        {
            _skillTestService = skillTestService;
        }

        // 1. GET: /SkillTests
        // Shows all tests for the logged-in user
        public async Task<IActionResult> Index()
        {
            // Note: Replace with your actual logged-in user identification logic 
            // e.g., int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            int mockUserId = 1;

            var tests = await _skillTestService.GetTestsForUserAsync(mockUserId);
            return View(tests);
        }

        // 2. GET: /SkillTests/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var skillTest = await _skillTestService.GetSkillTestByIdAsync(id);
            if (skillTest == null)
            {
                return NotFound();
            }
            return View(skillTest);
        }

        // 3. GET: /SkillTests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /SkillTests/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Guard against CSRF attacks per assignment rule
        public async Task<IActionResult> Create([Bind("Name,Score,AchievedDate")] SkillTest skillTest)
        {
            if (ModelState.IsValid) // Server-side validation check
            {
                await _skillTestService.AddSkillTestAsync(skillTest);
                return RedirectToAction(nameof(Index));
            }
            return View(skillTest);
        }

        // 4. GET: /SkillTests/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var skillTest = await _skillTestService.GetSkillTestByIdAsync(id);
            if (skillTest == null)
            {
                return NotFound();
            }
            return View(skillTest);
        }

        // POST: /SkillTests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SkillTestId,Name,Score,AchievedDate")] SkillTest skillTest)
        {
            if (id != skillTest.SkillTestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update score and date via service layer
                await _skillTestService.UpdateScoreAsync(id, skillTest.Score);
                await _skillTestService.UpdateAchievedDateAsync(id, skillTest.AchievedDate);
                return RedirectToAction(nameof(Index));
            }
            return View(skillTest);
        }

        // 5. GET: /SkillTests/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var skillTest = await _skillTestService.GetSkillTestByIdAsync(id);
            if (skillTest == null)
            {
                return NotFound();
            }
            return View(skillTest);
        }

        // POST: /SkillTests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _skillTestService.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
