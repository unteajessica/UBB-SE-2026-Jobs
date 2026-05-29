using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.ChatService;

namespace PussyCats.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/chats")]
public class ChatsController : ControllerBase
{
    private readonly IChatService chat;

    public ChatsController(IChatService chat)
    {
        this.chat = chat;
    }

    [HttpGet]
    public async Task<IActionResult> GetChats(
        [FromQuery] int? userId,
        [FromQuery] int? companyId,
        [FromQuery] int callerId,
        CancellationToken cancellationToken)
    {
        if (userId.HasValue)
            return Ok(await chat.GetChatsForUserAsync(userId.Value, cancellationToken));

        if (companyId.HasValue)
            return Ok(await chat.GetChatsForCompanyAsync(companyId.Value, cancellationToken));

        return BadRequest("Provide userId or companyId.");
    }

    [HttpPost]
    public async Task<IActionResult> FindOrCreate([FromBody] FindOrCreateChatRequest body, CancellationToken cancellationToken)
    {
        if (body.SecondUserId.HasValue)
        {
            var created = await chat.FindOrCreateUserChatAsync(body.UserId, body.SecondUserId.Value, cancellationToken);
            return Ok(created);
        }

        if (body.Company != null)
        {
            var created = await chat.FindOrCreateUserCompanyChatAsync(body.UserId, body.Company, body.Job, cancellationToken);
            return Ok(created);
        }

        return BadRequest("Provide SecondUserId or Company.");
    }

    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int callerId, CancellationToken cancellationToken)
    {
        try
        {
            var messages = await chat.GetMessagesAsync(id, callerId, cancellationToken);
            return Ok(messages);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException unauthorizedException)
        {
            return Problem(detail: unauthorizedException.Message, statusCode: 403);
        }
    }

    [HttpPost("{id}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageRequest body, CancellationToken cancellationToken)
    {
        try
        {
            await chat.SendMessageAsync(id, body.Content, body.SenderId, body.Type, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException argumentException)
        {
            return ValidationProblem(argumentException.Message);
        }
        catch (InvalidOperationException invalidOperationException)
        {
            return Problem(detail: invalidOperationException.Message, statusCode: 422);
        }
    }

    [HttpPost("{id}/attachments")]
    public async Task<IActionResult> SendStoredAttachment(int id, [FromBody] SendStoredAttachmentRequest body, CancellationToken cancellationToken)
    {
        try
        {
            await chat.SendStoredAttachmentAsync(id, body.StoredPath, body.OriginalFileName, body.SenderId, body.Type, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException invalidOperationException)
        {
            return Problem(detail: invalidOperationException.Message, statusCode: 422);
        }
    }

    [HttpPost("{id}/messages/read")]
    public async Task<IActionResult> MarkRead(int id, [FromQuery] int readerId, CancellationToken cancellationToken)
    {
        await chat.MarkMessagesAsReadAsync(id, readerId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id}/block")]
    public async Task<IActionResult> Block(int id, [FromBody] CallerRequest body, CancellationToken cancellationToken)
    {
        try
        {
            await chat.BlockChatAsync(id, body.CallerId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id}/unblock")]
    public async Task<IActionResult> Unblock(int id, [FromBody] CallerRequest body, CancellationToken cancellationToken)
    {
        try
        {
            await chat.UnblockChatAsync(id, body.CallerId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException unauthorizedException)
        {
            return Problem(detail: unauthorizedException.Message, statusCode: 403);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] int callerId, CancellationToken cancellationToken)
    {
        try
        {
            await chat.DeleteChatAsync(id, callerId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("search/users")]
    public async Task<IActionResult> SearchUsers([FromQuery] string userQuery, CancellationToken cancellationToken)
        => Ok(await chat.SearchUsersAsync(userQuery ?? string.Empty, cancellationToken));

    [HttpGet("search/companies")]
    public async Task<IActionResult> SearchCompanies([FromQuery] string companyQuery, CancellationToken cancellationToken)
        => Ok(await chat.SearchCompaniesAsync(companyQuery ?? string.Empty, cancellationToken));

    public record FindOrCreateChatRequest(int UserId, int? SecondUserId, Company? Company, Job? Job);
    public record SendMessageRequest(int SenderId, string Content, MessageType Type);
    public record SendStoredAttachmentRequest(int SenderId, string StoredPath, string OriginalFileName, MessageType Type);
    public record CallerRequest(int CallerId);
}
