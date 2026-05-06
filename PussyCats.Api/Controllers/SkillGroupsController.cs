using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/skill-groups")]
public class SkillGroupsController : ControllerBase
{
    private readonly ISkillGroupRepository skillGroups;

    public SkillGroupsController(ISkillGroupRepository skillGroups)
    {
        this.skillGroups = skillGroups;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] JobRole? jobRole, CancellationToken ct)
    {
        if (jobRole.HasValue)
            return Ok(await skillGroups.GetByJobRoleAsync(jobRole.Value, ct));

        return Ok(await skillGroups.GetAllAsync(ct));
    }
}
