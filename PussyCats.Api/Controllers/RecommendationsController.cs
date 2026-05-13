using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Jobs;
using PussyCats.Library.Repositories.Recommendations;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationRepository recommendations;
    private readonly IUserRepository users;
    private readonly IJobRepository jobs;

    public RecommendationsController(
        IRecommendationRepository recommendations,
        IUserRepository users,
        IJobRepository jobs)
    {
        this.recommendations = recommendations;
        this.users = users;
        this.jobs = jobs;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var recommendation = await recommendations.GetByIdAsync(id, cancellationToken);
        return recommendation is null ? NotFound() : Ok(recommendation);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? userId, [FromQuery] int? jobId, CancellationToken cancellationToken)
    {
        if (userId.HasValue && jobId.HasValue)
        {
            var recommendation = await recommendations.GetLatestByUserIdAndJobIdAsync(userId.Value, jobId.Value, cancellationToken);
            return Ok(recommendation);
        }

        return Ok(await recommendations.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateRecommendationRequest body, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(body.UserId, cancellationToken);
        if (user is null)
            return NotFound($"User {body.UserId} not found.");

        var job = await jobs.GetByIdAsync(body.JobId, cancellationToken);
        if (job is null)
            return NotFound($"Job {body.JobId} not found.");

        var recommendation = new Recommendation
        {
            User = user,
            Job = job,
            Timestamp = body.Timestamp == default ? DateTime.UtcNow : body.Timestamp,
        };

        var saved = await recommendations.AddAsync(recommendation, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = saved.RecommendationId }, saved);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (await recommendations.GetByIdAsync(id, cancellationToken) is null)
            return NotFound();

        await recommendations.RemoveAsync(id, cancellationToken);
        return NoContent();
    }

    public record CreateRecommendationRequest(int UserId, int JobId, DateTime Timestamp);
}
