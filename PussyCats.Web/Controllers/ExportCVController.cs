using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Services.UserProfileService;

namespace PussyCats.Web.Controllers;

public class ExportCVController : Controller
{
    private readonly IUserProfileService userProfile;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public ExportCVController(IUserProfileService userProfile)
    {
        this.userProfile = userProfile;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await userProfile.GetProfileAsync(CurrentUserId, cancellationToken);
        if (profile is null)
            return NotFound();
        return View(profile);
    }
}
