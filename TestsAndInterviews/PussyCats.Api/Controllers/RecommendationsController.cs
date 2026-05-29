using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Recommendations;
using PussyCats.Library.Services.UserRecommendationService;

namespace PussyCats.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService recommendations;
    private readonly IUserRecommendationService userRecommendationService; // Injected matchmaking service

    public RecommendationsController(IRecommendationService recommendations, IUserRecommendationService userRecommendationService)
    {
        this.recommendations = recommendations;
        this.userRecommendationService = userRecommendationService;
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
        catch (KeyNotFoundException exception)
        {
            return NotFound(exception.Message);
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

    [HttpPost("{userId}/next")]
    public async Task<IActionResult> GetNextCard(int userId, [FromBody] UserMatchmakingFilters filters, CancellationToken cancellationToken)
    {
        var card = await userRecommendationService.GetNextCardAsync(userId, filters, cancellationToken);
        if (card is null)
        {
            return NoContent();
        }
        return Ok(card);
    }

    [HttpPost("{userId}/fallback")]
    public async Task<IActionResult> GetFallbackCard(int userId, [FromBody] UserMatchmakingFilters filters, CancellationToken cancellationToken)
    {
        var card = await userRecommendationService.RecalculateTopCardIgnoringCooldownAsync(userId, filters, cancellationToken);
        if (card is null)
        {
            return NoContent();
        }
        return Ok(card);
    }

    [HttpPost("{userId}/like")]
    public async Task<IActionResult> ApplyLike(int userId, [FromBody] JobRecommendationResult card, CancellationToken cancellationToken)
    {
        try
        {
            int matchId = await userRecommendationService.ApplyLikeAsync(userId, card, cancellationToken);
            return Ok(matchId);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("{userId}/dismiss")]
    public async Task<IActionResult> ApplyDismiss(int userId, [FromBody] JobRecommendationResult card, CancellationToken cancellationToken)
    {
        int dismissRecommendationId = await userRecommendationService.ApplyDismissAsync(userId, card, cancellationToken);
        return Ok(dismissRecommendationId);
    }

    [HttpPost("undo-like")]
    public async Task<IActionResult> UndoLike([FromQuery] int matchId, [FromQuery] int? displayId, CancellationToken cancellationToken)
    {
        await userRecommendationService.UndoLikeAsync(matchId, displayId, cancellationToken);
        return NoContent();
    }

    [HttpPost("undo-dismiss")]
    public async Task<IActionResult> UndoDismiss([FromQuery] int dismissId, [FromQuery] int? displayId, CancellationToken cancellationToken)
    {
        await userRecommendationService.UndoDismissAsync(dismissId, displayId, cancellationToken);
        return NoContent();
    }

    public record CreateRecommendationRequest(int UserId, int JobId, DateTime Timestamp);
    public record UpdateRecommendationRequest(DateTime Timestamp);
}
