using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.ChatService;
using PussyCats.Library.Services.FileStorage;

namespace PussyCats.Web.Controllers;

public class ChatController : Controller
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png" };

    private readonly IChatService chat;
    private readonly ILocalFileStorageService fileStorage;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public ChatController(IChatService chat, ILocalFileStorageService fileStorage)
    {
        this.chat = chat;
        this.fileStorage = fileStorage;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var chats = await chat.GetChatsForUserAsync(CurrentUserId, cancellationToken);
        return View(chats);
    }

    public async Task<IActionResult> Show(int id, CancellationToken cancellationToken)
    {
        var messages = await chat.GetMessagesAsync(id, CurrentUserId, cancellationToken);
        await chat.MarkMessagesAsReadAsync(id, CurrentUserId, cancellationToken);
        ViewBag.ChatId = id;
        ViewBag.CurrentUserId = CurrentUserId;
        ViewBag.ApiBase = fileStorage.GetFilePath(string.Empty).TrimEnd('/');
        return View(messages);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id, string content, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(content))
            await chat.SendMessageAsync(id, content, CurrentUserId, MessageType.Text, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SendAttachment(int id, IFormFile attachment, CancellationToken cancellationToken)
    {
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
            await chat.SendStoredAttachmentAsync(id, path, attachment.FileName, CurrentUserId, type, cancellationToken);
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
        await chat.DeleteChatAsync(id, CurrentUserId, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(int id, CancellationToken cancellationToken)
    {
        await chat.BlockChatAsync(id, CurrentUserId, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(int id, CancellationToken cancellationToken)
    {
        await chat.UnblockChatAsync(id, CurrentUserId, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }
}
