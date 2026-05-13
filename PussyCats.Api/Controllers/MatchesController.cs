using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Jobs;
using PussyCats.Library.Repositories.Matches;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly IMatchRepository matches;
    private readonly IUserRepository userRepo;
    private readonly IJobRepository jobRepo;

    public MatchesController(IMatchRepository matches, IUserRepository userRepo, IJobRepository jobRepo)
    {
        this.matches = matches;
        this.userRepo = userRepo;
        this.jobRepo = jobRepo;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var match = await matches.GetByIdAsync(id, cancellationToken);
        return match is null ? NotFound() : Ok(match);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? userId, [FromQuery] int? jobId, CancellationToken cancellationToken)
    {
        if (userId.HasValue && jobId.HasValue)
        {
            var match = await matches.GetByUserIdAndJobIdAsync(userId.Value, jobId.Value, cancellationToken);
            return Ok(match is null ? Array.Empty<Match>() : new[] { match });
        }

        if (userId.HasValue)
            return Ok(await matches.GetByUserIdAsync(userId.Value, cancellationToken));

        return Ok(await matches.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateMatchRequest body, CancellationToken cancellationToken)
    {
        if (await matches.GetByUserIdAndJobIdAsync(body.UserId, body.JobId, cancellationToken) is not null)
            return Problem(detail: "A match already exists for this user and job.", statusCode: 409);

        var user = await userRepo.GetByIdAsync(body.UserId, cancellationToken);
        if (user is null)
            return NotFound($"User {body.UserId} not found.");

        var job = await jobRepo.GetByIdAsync(body.JobId, cancellationToken);
        if (job is null)
            return NotFound($"Job {body.JobId} not found.");

        var match = new Match
        {
            User = user,
            Job = job,
            Status = MatchStatus.Applied,
            Timestamp = DateTime.UtcNow,
            FeedbackMessage = string.Empty,
        };

        var saved = await matches.AddAsync(match, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = saved.MatchId }, saved);
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
        await matches.RemoveAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/decision")]
    public async Task<IActionResult> SubmitDecision(int id, [FromBody] SubmitDecisionRequest body, CancellationToken cancellationToken)
    {
        var match = await matches.GetByIdAsync(id, cancellationToken);
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
        await matches.UpdateAsync(match, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/advance")]
    public async Task<IActionResult> Advance(int id, CancellationToken cancellationToken)
    {
        var match = await matches.GetByIdAsync(id, cancellationToken);
        if (match is null)
            return NotFound();

        if (match.Status != MatchStatus.Applied)
            return Problem(
                detail: $"Cannot advance match with status {match.Status}. Expected Applied.",
                statusCode: 422);

        match.Status = MatchStatus.Advanced;
        match.Timestamp = DateTime.UtcNow;
        await matches.UpdateAsync(match, cancellationToken);
        return NoContent();
    }

    public record CreateMatchRequest(int UserId, int JobId);
    public record SubmitDecisionRequest(MatchStatus Decision, string? Feedback);
}
