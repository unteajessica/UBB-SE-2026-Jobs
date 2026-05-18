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

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await service.GetAllAsync(ct));

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var skill = await service.GetByIdAsync(id, ct);
        return skill is null ? NotFound() : View(skill);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Skill model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        await service.AddAsync(model, ct);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var skill = await service.GetByIdAsync(id, ct);
        return skill is null ? NotFound() : View(skill);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Skill model, CancellationToken ct)
    {
        if (id != model.SkillId) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        await service.UpdateAsync(model, ct);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var skill = await service.GetByIdAsync(id, ct);
        return skill is null ? NotFound() : View(skill);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        await service.RemoveAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }
}