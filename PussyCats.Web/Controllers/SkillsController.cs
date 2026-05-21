using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Skills;

namespace PussyCats.Web.Controllers;

//[Authorize]
public class SkillsController : Controller
{
    private readonly ISkillService service;

    public SkillsController(ISkillService service)
    {
        this.service = service;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await service.GetAllAsync(cancellationToken));

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var skill = await service.GetByIdAsync(id, cancellationToken);
        return skill is null ? NotFound() : View(skill);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Skill model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        await service.AddAsync(model, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var skill = await service.GetByIdAsync(id, cancellationToken);
        return skill is null ? NotFound() : View(skill);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Skill model, CancellationToken cancellationToken)
    {
        if (id != model.SkillId) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        await service.UpdateAsync(model, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var skill = await service.GetByIdAsync(id, cancellationToken);
        return skill is null ? NotFound() : View(skill);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await service.RemoveAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}