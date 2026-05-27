using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Services.Preferences;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers;

[Authorize]
public class PreferencesController : Controller
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private readonly IPreferenceService preferences;

    public PreferencesController(IPreferenceService preferences)
    {
        this.preferences = preferences;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var prefs = await preferences.GetByUserIdAsync(CurrentUserId, cancellationToken);
        return View(prefs);
    }

    public async Task<IActionResult> Edit(CancellationToken cancellationToken)
    {
        var prefs = await preferences.GetByUserIdAsync(CurrentUserId, cancellationToken);
        var model = new PreferencesEditModel
        {
            SelectedRoles = prefs.Roles.ToList(),
            WorkMode = prefs.WorkMode,
            Location = prefs.Location,
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PreferencesEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await preferences.SavePreferencesAsync(
                CurrentUserId,
                model.SelectedRoles,
                model.WorkMode,
                model.Location ?? string.Empty,
                cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}
