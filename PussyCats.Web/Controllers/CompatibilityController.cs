using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.CompatibilityService;

namespace PussyCats.Web.Controllers;

public class CompatibilityController : Controller
{
    private readonly ICompatibilityService compatibility;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public CompatibilityController(ICompatibilityService compatibility)
    {
        this.compatibility = compatibility;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var results = await compatibility.CalculateAllAsync(CurrentUserId, cancellationToken);
        return View(results);
    }

    public async Task<IActionResult> Detail(JobRole role, CancellationToken cancellationToken)
    {
        var result = await compatibility.CalculateForRoleAsync(CurrentUserId, role, cancellationToken);
        return View(result);
    }
}
