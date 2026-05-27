using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Developers;

namespace PussyCats.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/developer")]
public class DeveloperController : ControllerBase
{
    private readonly IDeveloperService developer;

    public DeveloperController(IDeveloperService developer)
    {
        this.developer = developer;
    }

    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts(CancellationToken cancellationToken)
        => Ok(await developer.GetPostsAsync(cancellationToken));

    [HttpGet("interactions")]
    public async Task<IActionResult> GetInteractions(CancellationToken cancellationToken)
        => Ok(await developer.GetInteractionsAsync(cancellationToken));

    [HttpGet("developers/{id:int}")]
    public async Task<IActionResult> GetDeveloper(int id, CancellationToken cancellationToken)
    {
        var result = await developer.GetDeveloperByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("posts")]
    public async Task<IActionResult> AddPost([FromBody] AddPostRequest body, CancellationToken cancellationToken)
    {
        try
        {
            var post = await developer.AddPostAsync(body.DeveloperId, body.ParameterType, body.Value, cancellationToken);
            return Ok(post);
        }
        catch (ArgumentException argumentException)
        {
            return ValidationProblem(argumentException.Message);
        }
    }

    [HttpPost("interactions")]
    public async Task<IActionResult> AddInteraction([FromBody] AddInteractionRequest body, CancellationToken cancellationToken)
    {
        await developer.AddInteractionAsync(body.DeveloperId, body.PostId, body.Type, cancellationToken);
        return NoContent();
    }

    [HttpDelete("interactions/{id:int}")]
    public async Task<IActionResult> RemoveInteraction(int id, CancellationToken cancellationToken)
    {
        await developer.RemoveInteractionAsync(id, cancellationToken);
        return NoContent();
    }

    public record AddPostRequest(int DeveloperId, DeveloperPostParameterType ParameterType, string Value);
    public record AddInteractionRequest(int DeveloperId, int PostId, DeveloperInteractionType Type);
}
