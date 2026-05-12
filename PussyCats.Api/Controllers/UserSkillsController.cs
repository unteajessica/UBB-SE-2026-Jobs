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
    public async Task<IActionResult> GetByUserId(int userId, CancellationToken cancellationToken)
        => Ok(await userSkills.GetByUserIdAsync(userId, cancellationToken));

    [HttpGet("verified")]
    public async Task<IActionResult> GetVerifiedByUserId(int userId, CancellationToken cancellationToken)
        => Ok(await userSkills.GetVerifiedByUserIdAsync(userId, cancellationToken));

    [HttpGet("{skillId}")]
    public async Task<IActionResult> GetBySkillId(int userId, int skillId, CancellationToken cancellationToken)
    {
        var userSkill = await userSkills.GetAsync(userId, skillId, cancellationToken);
        return userSkill is null ? NotFound() : Ok(userSkill);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int userId, [FromBody] UserSkill userSkill, CancellationToken cancellationToken)
    {
        userSkill.User = new User { UserId = userId };
        var saved = await userSkills.AddAsync(userSkill, cancellationToken);
        return CreatedAtAction(nameof(GetBySkillId), new { userId, skillId = saved.Skill.SkillId }, saved);
    }

    [HttpPut("{skillId}")]
    public async Task<IActionResult> Update(int userId, int skillId, [FromBody] UserSkill userSkill, CancellationToken cancellationToken)
    {
        if (await userSkills.GetAsync(userId, skillId, cancellationToken) is null)
            return NotFound();
        userSkill.User = new User { UserId = userId };
        userSkill.Skill = new Skill { SkillId = skillId };
        await userSkills.UpdateAsync(userSkill, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{skillId}/score")]
    public async Task<IActionResult> UpdateScore(int userId, int skillId, [FromBody] UpdateScoreRequest body, CancellationToken cancellationToken)
    {
        if (await userSkills.GetAsync(userId, skillId, cancellationToken) is null)
            return NotFound();
        await userSkills.UpdateScoreAsync(userId, skillId, body.Score, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{skillId}")]
    public async Task<IActionResult> Remove(int userId, int skillId, CancellationToken cancellationToken)
    {
        if (await userSkills.GetAsync(userId, skillId, cancellationToken) is null)
            return NotFound();
        await userSkills.RemoveAsync(userId, skillId, cancellationToken);
        return NoContent();
    }

    public record UpdateScoreRequest(int Score);
}
