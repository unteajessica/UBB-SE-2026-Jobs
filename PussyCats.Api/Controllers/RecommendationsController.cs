using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Services.Recommendations;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService recommendations;

    public RecommendationsController(IRecommendationService recommendations)
    {
        this.recommendations = recommendations;
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
            var recommendation = await recommendations.GetLatestForUserAndJobAsync(userId.Value, jobId.Value, cancellationToken);
            return Ok(recommendation);
        }

        return Ok(await recommendations.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateRecommendationRequest body, CancellationToken cancellationToken)
    {
        try
        {
            DateTime? timestamp = body.Timestamp == default ? null : body.Timestamp;
            var saved = await recommendations.AddAsync(body.UserId, body.JobId, timestamp, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = saved.RecommendationId }, saved);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRecommendationRequest body, CancellationToken cancellationToken)
    {
        try
        {
            await recommendations.UpdateTimestampAsync(id, body.Timestamp, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
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
    public record UpdateRecommendationRequest(DateTime Timestamp);
}
