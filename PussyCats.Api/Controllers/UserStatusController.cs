using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Services.UserStatusService;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/user-status")]
public class UserStatusController : ControllerBase
{
    private readonly IUserStatusService userStatus;

    public UserStatusController(IUserStatusService userStatus)
    {
        this.userStatus = userStatus;
    }

    [HttpGet("{userId}/applications")]
    public async Task<IActionResult> GetApplications(int userId, CancellationToken cancellationToken)
        => Ok(await userStatus.GetApplicationsForUserAsync(userId, cancellationToken));
}
