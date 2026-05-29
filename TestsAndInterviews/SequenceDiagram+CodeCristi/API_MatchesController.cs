using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Matches;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly IMatchService matches;

    public MatchesController(IMatchService matches)
    {
        this.matches = matches;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var match = await matches.GetByIdAsync(id, cancellationToken);
        return match is null ? NotFound() : Ok(match);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? userId,
        [FromQuery] int? jobId,
        [FromQuery] int? companyId,
        CancellationToken cancellationToken)
    {
        if (userId.HasValue && jobId.HasValue)
        {
            var match = await matches.GetByUserIdAndJobIdAsync(userId.Value, jobId.Value, cancellationToken);
            return Ok(match is null ? Array.Empty<Match>() : new[] { match });
        }

        if (userId.HasValue)
            return Ok(await matches.GetMatchesForUserAsync(userId.Value, cancellationToken));

        if (companyId.HasValue)
            return Ok(await matches.GetByCompanyIdAsync(companyId.Value, cancellationToken));

        return Ok(await matches.GetAllMatchesAsync(cancellationToken));
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics([FromQuery] int userId, CancellationToken cancellationToken)
    {
        return Ok(await matches.GetMatchStatisticsAsync(userId, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateMatchRequest body, CancellationToken cancellationToken)
    {
        try
        {
            var matchId = await matches.CreatePendingApplicationAsync(body.UserId, body.JobId, cancellationToken);
            var created = await matches.GetByIdAsync(matchId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = matchId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, statusCode: 409);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Match match, CancellationToken cancellationToken)
    {
        if (await matches.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();

        match.MatchId = id;
        await matches.UpdateAsync(match, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (await matches.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();
        await matches.RemoveApplicationAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/decision")]
    public async Task<IActionResult> SubmitDecision(int id, [FromBody] SubmitDecisionRequest body, CancellationToken cancellationToken)
    {
        try
        {
            await matches.SubmitDecisionAsync(id, body.Decision, body.Feedback ?? string.Empty, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return Problem(detail: ex.Message, statusCode: 400);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, statusCode: 422);
        }
    }

    [HttpPatch("{id}/advance")]
    public async Task<IActionResult> Advance(int id, CancellationToken cancellationToken)
    {
        try
        {
            await matches.AdvanceAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Problem(detail: ex.Message, statusCode: 422);
        }
    }

    [HttpPatch("{id}/revert")]
    public async Task<IActionResult> Revert(int id, CancellationToken cancellationToken)
    {
        try
        {
            await matches.RevertToAppliedAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public record CreateMatchRequest(int UserId, int JobId);
    public record SubmitDecisionRequest(MatchStatus Decision, string? Feedback);
}
