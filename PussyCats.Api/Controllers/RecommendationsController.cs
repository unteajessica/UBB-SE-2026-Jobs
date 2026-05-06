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
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var recommendation = await recommendations.GetByIdAsync(id, ct);
        return recommendation is null ? NotFound() : Ok(recommendation);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? userId, [FromQuery] int? jobId, CancellationToken ct)
    {
        if (userId.HasValue && jobId.HasValue)
        {
            var recommendation = await recommendations.GetLatestByUserIdAndJobIdAsync(userId.Value, jobId.Value, ct);
            return Ok(recommendation);
        }

        return Ok(await recommendations.GetAllAsync(ct));
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Recommendation recommendation, CancellationToken ct)
    {
        var saved = await recommendations.AddAsync(recommendation, ct);
        return CreatedAtAction(nameof(GetById), new { id = saved.RecommendationId }, saved);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        if (await recommendations.GetByIdAsync(id, ct) is null)
            return NotFound();

        await recommendations.RemoveAsync(id, ct);
        return NoContent();
    }
}
