using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Recommendations;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationRepository recommendations;

    public RecommendationsController(IRecommendationRepository recommendations)
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
            var recommendation = await recommendations.GetLatestByUserIdAndJobIdAsync(userId.Value, jobId.Value, cancellationToken);
            return Ok(recommendation);
        }

        return Ok(await recommendations.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Recommendation recommendation, CancellationToken ct)
    {
        recommendation.RecommendationId = 0;
        var saved = await recommendations.AddAsync(recommendation, ct);
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
}
