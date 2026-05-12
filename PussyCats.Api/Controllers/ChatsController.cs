using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Chats;
using PussyCats.Library.Repositories.Companies;
using PussyCats.Library.Repositories.Messages;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/chats")]
public class ChatsController : ControllerBase
{
    private readonly IChatRepository chatRepo;
    private readonly IMessageRepository messageRepo;
    private readonly IUserRepository userRepo;
    private readonly ICompanyRepository companyRepo;

    public ChatsController(
        IChatRepository chatRepo,
        IMessageRepository messageRepo,
        IUserRepository userRepo,
        ICompanyRepository companyRepo)
    {
        this.chatRepo = chatRepo;
        this.messageRepo = messageRepo;
        this.userRepo = userRepo;
        this.companyRepo = companyRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetChats(
        [FromQuery] int? userId,
        [FromQuery] int? companyId,
        [FromQuery] int? callerId,
        CancellationToken cancellationToken)
    {
        if (userId.HasValue)
        {
            var chats = await chatRepo.GetForUserAsync(userId.Value, cancellationToken).ConfigureAwait(false);
            var enrichCaller = callerId ?? userId.Value;
            foreach (var chat in chats)
            {
                await EnrichChatAsync(chat, enrichCaller, cancellationToken).ConfigureAwait(false);
            }
            return Ok(chats);
        }

        if (companyId.HasValue)
        {
            var chats = await chatRepo.GetForCompanyAsync(companyId.Value, cancellationToken).ConfigureAwait(false);
            var enrichCaller = callerId ?? companyId.Value;
            foreach (var chat in chats)
            {
                await EnrichChatAsync(chat, enrichCaller, cancellationToken).ConfigureAwait(false);
            }
            return Ok(chats);
        }

        return BadRequest("Provide userId or companyId.");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var chat = await chatRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return chat is null ? NotFound() : Ok(chat);
    }

    [HttpPost]
    public async Task<IActionResult> FindOrCreate([FromBody] FindOrCreateChatRequest body, CancellationToken cancellationToken)
    {
        if (body.SecondUserId.HasValue)
        {
            var existing = await chatRepo.FindUserUserChatAsync(body.UserId, body.SecondUserId.Value, cancellationToken).ConfigureAwait(false);
            if (existing is not null)
                return Ok(existing);

            var created = await chatRepo.AddAsync(new Chat { UserId = body.UserId, SecondUserId = body.SecondUserId }, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = created.ChatId }, created);
        }

        if (body.CompanyId.HasValue)
        {
            var existing = await chatRepo.FindUserCompanyChatAsync(body.UserId, body.CompanyId.Value, body.JobId, cancellationToken).ConfigureAwait(false);
            if (existing is not null)
                return Ok(existing);

            var created = await chatRepo.AddAsync(
                new Chat { UserId = body.UserId, CompanyId = body.CompanyId, JobId = body.JobId },
                cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = created.ChatId }, created);
        }

        return BadRequest("Provide SecondUserId or CompanyId.");
    }

    [HttpPatch("{id}/block")]
    public async Task<IActionResult> Block(int id, [FromBody] BlockRequest body, CancellationToken cancellationToken)
    {
        var chat = await chatRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (chat is null)
            return NotFound();

        chat.IsBlocked = true;
        chat.BlockedByUserId = body.BlockerId;
        await chatRepo.UpdateAsync(chat, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPatch("{id}/unblock")]
    public async Task<IActionResult> Unblock(int id, [FromBody] UnblockRequest body, CancellationToken cancellationToken)
    {
        var chat = await chatRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (chat is null)
            return NotFound();

        if (chat.BlockedByUser?.UserId != body.UnblockerId)
            return Problem(detail: "Only the user who blocked this chat can unblock it.", statusCode: 403);

        chat.IsBlocked = false;
        chat.BlockedByUserId = null;
        await chatRepo.UpdateAsync(chat, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] int callerId, CancellationToken cancellationToken)
    {
        var chat = await chatRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (chat is null)
            return NotFound();

        if (chat.UserId == callerId)
            chat.DeletedAtByUser = DateTime.UtcNow;
        else
            chat.DeletedAtBySecondParty = DateTime.UtcNow;

        await chatRepo.UpdateAsync(chat, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Chat body, CancellationToken cancellationToken)
    {
        var existing = await chatRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (existing is null)
            return NotFound();

        existing.IsBlocked = body.IsBlocked;
        existing.BlockedByUserId = body.BlockedByUserId;
        existing.DeletedAtByUser = body.DeletedAtByUser;
        existing.DeletedAtBySecondParty = body.DeletedAtBySecondParty;
        await chatRepo.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetMessages(int id, CancellationToken cancellationToken)
    {
        var chat = await chatRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (chat is null)
            return NotFound();

        var messages = await messageRepo.GetForChatAsync(id, cancellationToken).ConfigureAwait(false);
        return Ok(messages);
    }

    [HttpPost("{id}/messages")]
    public async Task<IActionResult> AddMessage(int id, [FromBody] AddMessageRequest body, CancellationToken cancellationToken)
    {
        var chat = await chatRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (chat is null)
            return NotFound();

        var message = new Message
        {
            ChatId = id,
            SenderId = body.SenderId,
            Content = body.Content,
            Type = body.Type,
            OriginalFileName = body.OriginalFileName ?? string.Empty,
            Timestamp = DateTime.UtcNow,
            IsRead = false,
        };

        var saved = await messageRepo.AddAsync(message, cancellationToken).ConfigureAwait(false);
        return Ok(saved);
    }

    [HttpGet("{id}/messages/latest")]
    public async Task<IActionResult> GetLatestMessage(int id, CancellationToken cancellationToken)
    {
        var latest = await messageRepo.GetLatestForChatAsync(id, cancellationToken).ConfigureAwait(false);
        return latest is null ? NotFound() : Ok(latest);
    }

    [HttpGet("{id}/messages/unread")]
    public async Task<IActionResult> GetUnreadCount(int id, [FromQuery] int senderId, CancellationToken cancellationToken)
    {
        var count = await messageRepo.GetUnreadCountAsync(id, senderId, cancellationToken).ConfigureAwait(false);
        return Ok(count);
    }

    [HttpPost("{id}/messages/read")]
    public async Task<IActionResult> MarkRead(int id, [FromQuery] int readerId, CancellationToken cancellationToken)
    {
        var chat = await chatRepo.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (chat is null)
            return NotFound();

        await messageRepo.MarkAsReadAsync(id, readerId, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    private async Task EnrichChatAsync(Chat chat, int callerId, CancellationToken cancellationToken)
    {
        var latest = await messageRepo.GetLatestForChatAsync(chat.ChatId, cancellationToken).ConfigureAwait(false);
        if (latest is not null)
        {
            chat.LastMessage = latest.Type == MessageType.Text
                ? latest.Content
                : (!string.IsNullOrEmpty(latest.OriginalFileName)
                    ? latest.OriginalFileName
                    : Path.GetFileName(latest.Content));
            chat.LastMessageSnippet = chat.LastMessage.Length > 60
                ? chat.LastMessage[..57] + "..."
                : chat.LastMessage;
            chat.LastMessageTime = latest.Timestamp.ToLocalTime().Date == DateTime.Now.Date
                ? latest.Timestamp.ToLocalTime().ToString("HH:mm")
                : latest.Timestamp.ToLocalTime().ToString("dd MMM");
        }
        chat.UnreadCount = await messageRepo.GetUnreadCountAsync(chat.ChatId, callerId, cancellationToken).ConfigureAwait(false);
        chat.OtherPartyName = await ResolveOtherPartyNameAsync(chat, callerId, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> ResolveOtherPartyNameAsync(Chat chat, int callerId, CancellationToken cancellationToken)
    {
        if (chat.CompanyId.HasValue)
        {
            if (chat.UserId == callerId)
            {
                var company = await companyRepo.GetByIdAsync(chat.CompanyId.Value, cancellationToken).ConfigureAwait(false);
                return company?.CompanyName ?? $"Company {chat.CompanyId.Value}";
            }

            var user = await userRepo.GetByIdAsync(chat.UserId, cancellationToken).ConfigureAwait(false);
            return user is not null ? $"{user.FirstName} {user.LastName}".Trim() : $"User {chat.UserId}";
        }

        var otherUserId = chat.UserId == callerId ? chat.SecondUser?.UserId : chat.UserId;
        if (otherUserId.HasValue)
        {
            var otherUser = await userRepo.GetByIdAsync(otherUserId.Value, cancellationToken).ConfigureAwait(false);
            return otherUser is not null ? $"{otherUser.FirstName} {otherUser.LastName}".Trim() : $"User {otherUserId.Value}";
        }

        return "Conversation";
    }

    public record FindOrCreateChatRequest(int UserId, int? CompanyId, int? SecondUserId, int? JobId);
    public record BlockRequest(int BlockerId);
    public record UnblockRequest(int UnblockerId);
    public record AddMessageRequest(int SenderId, string Content, MessageType Type, string? OriginalFileName);
}
