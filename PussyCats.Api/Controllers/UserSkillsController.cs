using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/users/{userId}/skills")]
public class UserSkillsController : ControllerBase
{
    private readonly IUserSkillRepository userSkills;

    public UserSkillsController(IUserSkillRepository userSkills)
    {
        this.userSkills = userSkills;
    }

    [HttpGet]
    public async Task<IActionResult> GetByUserId(int userId, CancellationToken ct)
        => Ok(await userSkills.GetByUserIdAsync(userId, ct));

    [HttpGet("verified")]
    public async Task<IActionResult> GetVerifiedByUserId(int userId, CancellationToken ct)
        => Ok(await userSkills.GetVerifiedByUserIdAsync(userId, ct));

    [HttpGet("{skillId}")]
    public async Task<IActionResult> GetBySkillId(int userId, int skillId, CancellationToken ct)
    {
        var userSkill = await userSkills.GetAsync(userId, skillId, ct);
        return userSkill is null ? NotFound() : Ok(userSkill);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int userId, [FromBody] UserSkill userSkill, CancellationToken ct)
    {
        userSkill.UserId = userId;
        var saved = await userSkills.AddAsync(userSkill, ct);
        return CreatedAtAction(nameof(GetBySkillId), new { userId, skillId = saved.SkillId }, saved);
    }

    [HttpPut("{skillId}")]
    public async Task<IActionResult> Update(int userId, int skillId, [FromBody] UserSkill userSkill, CancellationToken ct)
    {
        if (await userSkills.GetAsync(userId, skillId, ct) is null)
            return NotFound();
        userSkill.UserId = userId;
        userSkill.SkillId = skillId;
        await userSkills.UpdateAsync(userSkill, ct);
        return NoContent();
    }

    [HttpPatch("{skillId}/score")]
    public async Task<IActionResult> UpdateScore(int userId, int skillId, [FromBody] UpdateScoreRequest body, CancellationToken ct)
    {
        if (await userSkills.GetAsync(userId, skillId, ct) is null)
            return NotFound();
        await userSkills.UpdateScoreAsync(userId, skillId, body.Score, ct);
        return NoContent();
    }

    [HttpDelete("{skillId}")]
    public async Task<IActionResult> Remove(int userId, int skillId, CancellationToken ct)
    {
        if (await userSkills.GetAsync(userId, skillId, ct) is null)
            return NotFound();
        await userSkills.RemoveAsync(userId, skillId, ct);
        return NoContent();
    }

    public record UpdateScoreRequest(int Score);
}
