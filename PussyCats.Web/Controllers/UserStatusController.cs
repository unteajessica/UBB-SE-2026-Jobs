using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.UserStatusService;

namespace PussyCats.Web.Controllers;

public class UserStatusController : Controller
{
    private readonly IUserStatusService userStatus;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public UserStatusController(IUserStatusService userStatus)
    {
        this.userStatus = userStatus;
    }

    public async Task<IActionResult> Index(string? filter, CancellationToken cancellationToken)
    {
        var applications = await userStatus.GetApplicationsForUserAsync(CurrentUserId, cancellationToken);

        var filtered = filter switch
        {
            "Applied"  => applications.Where(a => a.Status == MatchStatus.Applied).ToList(),
            "Accepted" => applications.Where(a => a.Status == MatchStatus.Accepted).ToList(),
            "Rejected" => applications.Where(a => a.Status == MatchStatus.Rejected).ToList(),
            _          => applications.ToList(),
        };

        ViewBag.CurrentFilter = filter ?? "All";
        ViewBag.TotalCount = applications.Count;
        return View(filtered);
    }
}
