using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Matches;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly IMatchRepository matches;

    public MatchesController(IMatchRepository matches)
    {
        this.matches = matches;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var match = await matches.GetByIdAsync(id, ct);
        return match is null ? NotFound() : Ok(match);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? userId, CancellationToken ct)
    {
        if (userId.HasValue)
            return Ok(await matches.GetByUserIdAsync(userId.Value, ct));
        return Ok(await matches.GetAllAsync(ct));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateMatchRequest body, CancellationToken ct)
    {
        if (await matches.GetByUserIdAndJobIdAsync(body.UserId, body.JobId, ct) is not null)
            return Problem(detail: "A match already exists for this user and job.", statusCode: 409);

        var match = new Match
        {
            UserId = body.UserId,
            JobId = body.JobId,
            Status = MatchStatus.Applied,
            Timestamp = DateTime.UtcNow,
            FeedbackMessage = string.Empty,
        };

        var saved = await matches.AddAsync(match, ct);
        return CreatedAtAction(nameof(GetById), new { id = saved.MatchId }, saved);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        if (await matches.GetByIdAsync(id, ct) is null)
            return NotFound();
        await matches.RemoveAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id}/decision")]
    public async Task<IActionResult> SubmitDecision(int id, [FromBody] SubmitDecisionRequest body, CancellationToken ct)
    {
        var match = await matches.GetByIdAsync(id, ct);
        if (match is null)
            return NotFound();

        if (!MatchStatusTransitions.IsDecisionTransitionAllowed(match.Status, body.Decision))
            return Problem(
                detail: $"Cannot transition match from {match.Status} to {body.Decision}.",
                statusCode: 422);

        if (body.Decision == MatchStatus.Rejected && string.IsNullOrWhiteSpace(body.Feedback))
            return Problem(detail: "Feedback is required when rejecting.", statusCode: 400);

        match.Status = body.Decision;
        match.FeedbackMessage = (body.Feedback ?? string.Empty).Trim();
        match.Timestamp = DateTime.UtcNow;
        await matches.UpdateAsync(match, ct);
        return NoContent();
    }

    [HttpPatch("{id}/advance")]
    public async Task<IActionResult> Advance(int id, CancellationToken ct)
    {
        var match = await matches.GetByIdAsync(id, ct);
        if (match is null)
            return NotFound();

        if (match.Status != MatchStatus.Applied)
            return Problem(
                detail: $"Cannot advance match with status {match.Status}. Expected Applied.",
                statusCode: 422);

        match.Status = MatchStatus.Advanced;
        match.Timestamp = DateTime.UtcNow;
        await matches.UpdateAsync(match, ct);
        return NoContent();
    }

    public record CreateMatchRequest(int UserId, int JobId);
    public record SubmitDecisionRequest(MatchStatus Decision, string? Feedback);
}
