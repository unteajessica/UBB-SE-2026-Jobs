using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Services.CooldownService;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/cooldown")]
public class CooldownController : ControllerBase
{
    private readonly ICooldownService cooldown;

    public CooldownController(ICooldownService cooldown)
    {
        this.cooldown = cooldown;
    }

    [HttpGet("users/{userId}/jobs/{jobId}")]
    public async Task<IActionResult> Check(int userId, int jobId, CancellationToken cancellationToken)
    {
        var isOnCooldown = await cooldown.IsOnCooldownAsync(userId, jobId, DateTime.UtcNow, cancellationToken);
        return Ok(new { isOnCooldown });
    }
}
