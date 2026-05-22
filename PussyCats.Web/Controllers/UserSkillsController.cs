using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.UserSkillService;

namespace PussyCats.Web.Controllers;

public class UserSkillsController : Controller
{
    private readonly IUserSkillService service;

    public UserSkillsController(IUserSkillService service)
    {
        this.service = service;
    }

    public async Task<IActionResult> Index(int userId, CancellationToken cancellationToken)
    {
        var list = await service.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        return View(list ?? Enumerable.Empty<UserSkill>());
    }

    public async Task<IActionResult> Details(int userId, int skillId, CancellationToken cancellationToken)
    {
        var userSkill = await service.GetAsync(userId, skillId, cancellationToken);
        return userSkill is null ? NotFound() : View(userSkill);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int userId, int skillId, UserSkill model, CancellationToken cancellationToken)
    {
        model.User = new User { UserId = userId };
        model.Skill = new Skill { SkillId = skillId };
        ModelState.ClearValidationState(nameof(model.User));
        ModelState.ClearValidationState(nameof(model.Skill));
        if (!TryValidateModel(model))
            return View(model);

        await service.AddAsync(model, cancellationToken);
        return RedirectToAction(nameof(Index), new { userId });
    }

    public async Task<IActionResult> Edit(int userId, int skillId, CancellationToken cancellationToken)
    {
        var userSkill = await service.GetAsync(userId, skillId, cancellationToken);
        return userSkill is null ? NotFound() : View(userSkill);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int userId, int skillId, UserSkill model, CancellationToken cancellationToken)
    {
        model.User = new User { UserId = userId };
        model.Skill = new Skill { SkillId = skillId };
        await service.UpdateAsync(model, cancellationToken);
        return RedirectToAction(nameof(Index), new { userId });
    }

    public async Task<IActionResult> Delete(int userId, int skillId, CancellationToken cancellationToken)
    {
        var userSkill = await service.GetAsync(userId, skillId, cancellationToken);
        return userSkill is null ? NotFound() : View(userSkill);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int userId, int skillId, CancellationToken cancellationToken)
    {
        await service.RemoveAsync(userId, skillId, cancellationToken);
        return RedirectToAction(nameof(Index), new { userId });
    }
}
