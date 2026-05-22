using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.ChatService;
using PussyCats.Library.Services.FileStorage;
using PussyCats.Web.Configuration;
using PussyCats.Web.Infrastructure;

namespace PussyCats.Web.Controllers;

public class ChatController : Controller
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png" };

    private readonly IChatService chat;
    private readonly ILocalFileStorageService fileStorage;
    private readonly ApiConfiguration apiConfiguration;

    public ChatController(IChatService chat, ILocalFileStorageService fileStorage, ApiConfiguration apiConfiguration)
    {
        this.chat = chat;
        this.fileStorage = fileStorage;
        this.apiConfiguration = apiConfiguration;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var callerId = GetCallerId();
        var isCompanyMode = IsCompanyMode();
        var chats = isCompanyMode
            ? await chat.GetChatsForCompanyAsync(callerId, cancellationToken)
            : await chat.GetChatsForUserAsync(callerId, cancellationToken);
        return View(chats);
    }

    public async Task<IActionResult> Show(int id, CancellationToken cancellationToken)
    {
        var callerId = GetCallerId();
        var messages = await chat.GetMessagesAsync(id, callerId, cancellationToken);
        await chat.MarkMessagesAsReadAsync(id, callerId, cancellationToken);
        ViewBag.ChatId = id;
        ViewBag.CurrentUserId = callerId;
        ViewBag.ApiBase = apiConfiguration.BaseUrl.TrimEnd('/') + "/api/files";
        return View(messages);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id, string content, CancellationToken cancellationToken)
    {
        var callerId = GetCallerId();
        if (!string.IsNullOrWhiteSpace(content))
            await chat.SendMessageAsync(id, content, callerId, MessageType.Text, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SendAttachment(int id, IFormFile attachment, CancellationToken cancellationToken)
    {
        var callerId = GetCallerId();
        if (attachment is null || attachment.Length == 0)
        {
            TempData["ChatError"] = "No file selected.";
            return RedirectToAction(nameof(Show), new { id });
        }

        try
        {
            await using var stream = attachment.OpenReadStream();
            var path = await fileStorage.SaveFileAsync(stream, attachment.FileName, cancellationToken);
            var ext = Path.GetExtension(attachment.FileName);
            var type = ImageExtensions.Contains(ext) ? MessageType.Image : MessageType.File;
            await chat.SendStoredAttachmentAsync(id, path, attachment.FileName, callerId, type, cancellationToken);
        }
        catch (Exception ex)
        {
            TempData["ChatError"] = ex.Message;
        }

        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var callerId = GetCallerId();
        await chat.DeleteChatAsync(id, callerId, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(int id, CancellationToken cancellationToken)
    {
        var callerId = GetCallerId();
        await chat.BlockChatAsync(id, callerId, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(int id, CancellationToken cancellationToken)
    {
        var callerId = GetCallerId();
        await chat.UnblockChatAsync(id, callerId, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }

    private int GetCallerId()
    {
        if (IsCompanyMode())
        {
            return apiConfiguration.TemporaryCompanyId;
        }

        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idValue))
        {
            throw new InvalidOperationException("User session is missing a user id.");
        }

        return int.Parse(idValue, System.Globalization.CultureInfo.InvariantCulture);
    }

    private bool IsCompanyMode()
        => string.Equals(HttpContext.Session.GetString(SessionKeys.Mode), AppModes.Company, StringComparison.OrdinalIgnoreCase);
}
