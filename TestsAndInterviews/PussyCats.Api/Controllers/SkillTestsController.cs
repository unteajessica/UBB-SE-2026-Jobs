using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.SkillTests;

namespace PussyCats.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/skill-tests")]
public class SkillTestsController : ControllerBase
{
    private readonly ISkillTestService skillTests;

    public SkillTestsController(ISkillTestService skillTests)
    {
        this.skillTests = skillTests;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var skillTest = await skillTests.GetSkillTestByIdAsync(id, cancellationToken);
        return skillTest is null ? NotFound() : Ok(skillTest);
    }

    [HttpGet("{id}/retake-eligibility")]
    public async Task<IActionResult> GetRetakeEligibility(int id, CancellationToken cancellationToken)
    {
        try
        {
            bool canRetake = await skillTests.CanRetakeTestAsync(id, cancellationToken);
            return Ok(new { CanRetake = canRetake });

        }
        catch (Exception exception)
        {
            return NotFound(new { Message = exception.Message });
        }
    }

    [HttpPost("{id}/retake")]
    public async Task<IActionResult> SubmitRetake(int id, [FromBody] RetakeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var badge = await skillTests.SubmitRetakeAsync(id, request.Score, cancellationToken);
            return Ok(badge);
        }
        catch (Exception exception)
        {
            return NotFound(new { Message = exception.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetByUserId([FromQuery] int userId, CancellationToken cancellationToken)
    {
        return Ok(await skillTests.GetTestsForUserAsync(userId, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] SkillTest skillTest, CancellationToken cancellationToken)
    {
        skillTest.SkillTestId = 0;
        var saved = await skillTests.AddSkillTestAsync(skillTest, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = saved.SkillTestId }, saved);
    }

    [HttpPatch("{id}/score")]
    public async Task<IActionResult> UpdateScore(int id, [FromBody] UpdateScoreRequest body, CancellationToken cancellationToken)
    {
        if (await skillTests.GetSkillTestByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await skillTests.UpdateScoreAsync(id, body.Score, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/date")]
    public async Task<IActionResult> UpdateDate(int id, [FromBody] UpdateDateRequest body, CancellationToken cancellationToken)
    {
        if (await skillTests.GetSkillTestByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await skillTests.UpdateAchievedDateAsync(id, body.AchievedDate, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (await skillTests.GetSkillTestByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await skillTests.RemoveAsync(id, cancellationToken);
        return NoContent();
    }

    public record UpdateScoreRequest(int Score);
    public record UpdateDateRequest(DateOnly AchievedDate);
    public record RetakeRequest(int Score);
}
