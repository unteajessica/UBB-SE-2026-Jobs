using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.SkillTests;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/skill-tests")]
public class SkillTestsController : ControllerBase
{
    private readonly ISkillTestRepository skillTests;

    public SkillTestsController(ISkillTestRepository skillTests)
    {
        this.skillTests = skillTests;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var skillTest = await skillTests.GetByIdAsync(id, cancellationToken);
        return skillTest is null ? NotFound() : Ok(skillTest);
    }

    [HttpGet]
    public async Task<IActionResult> GetByUserId([FromQuery] int userId, CancellationToken cancellationToken)
        => Ok(await skillTests.GetByUserIdAsync(userId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] SkillTest skillTest, CancellationToken cancellationToken)
    {
        skillTest.SkillTestId = 0;
        var saved = await skillTests.AddAsync(skillTest, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = saved.SkillTestId }, saved);
    }

    [HttpPatch("{id}/score")]
    public async Task<IActionResult> UpdateScore(int id, [FromBody] UpdateScoreRequest body, CancellationToken cancellationToken)
    {
        if (await skillTests.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await skillTests.UpdateScoreAsync(id, body.Score, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/date")]
    public async Task<IActionResult> UpdateDate(int id, [FromBody] UpdateDateRequest body, CancellationToken cancellationToken)
    {
        if (await skillTests.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await skillTests.UpdateAchievedDateAsync(id, body.AchievedDate, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (await skillTests.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await skillTests.RemoveAsync(id, cancellationToken);
        return NoContent();
    }

    public record UpdateScoreRequest(int Score);
    public record UpdateDateRequest(DateOnly AchievedDate);
}
