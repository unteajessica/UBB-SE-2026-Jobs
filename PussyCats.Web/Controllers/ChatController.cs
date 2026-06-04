using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.ChatService;
using PussyCats.Library.Services.FileStorage;
using PussyCats.Web.Configuration;
using PussyCats.Web.Infrastructure;

namespace PussyCats.Web.Controllers;

[Authorize]
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

    public async Task<IActionResult> Index(string? tab, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isCompanyMode = IsCompanyMode();

        if (isCompanyMode)
        {
            var companyId = GetCompanyId();
            var chats = await chat.GetChatsForCompanyAsync(companyId, cancellationToken);
            ViewBag.IsCompanyMode = true;
            ViewBag.ActiveTab = (tab == "companies") ? "companies" : "users";
            return View(chats);
        }

        var userChats = await chat.GetChatsForUserAsync(userId, cancellationToken);
        ViewBag.IsCompanyMode = false;
        ViewBag.ActiveTab = (tab == "companies") ? "companies" : "users";
        return View(userChats);
    }

    public async Task<IActionResult> Show(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isCompanyMode = IsCompanyMode();
        int? companyId = null;

        // Verify authorization for company chats
        if (isCompanyMode)
        {
            companyId = GetCompanyId();
            var chatExists = await VerifyChatAuthorizationAsync(id, companyId.Value, cancellationToken);
            if (!chatExists)
            {
                return Forbid();
            }
        }

        var messages = await chat.GetMessagesAsync(id, userId, companyId, cancellationToken);
        await chat.MarkMessagesAsReadAsync(id, userId, cancellationToken);
        ViewBag.ChatId = id;
        ViewBag.CurrentUserId = userId;
        ViewBag.ApiBase = apiConfiguration.BaseUrl.TrimEnd('/') + "/api/files";
        return View(messages);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id, string content, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isCompanyMode = IsCompanyMode();
        int? companyId = null;

        if (isCompanyMode)
        {
            companyId = GetCompanyId();
            var chatExists = await VerifyChatAuthorizationAsync(id, companyId.Value, cancellationToken);
            if (!chatExists)
            {
                return Forbid();
            }
        }

        if (!string.IsNullOrWhiteSpace(content))
            await chat.SendMessageAsync(id, content, userId, MessageType.Text, companyId, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SendAttachment(int id, IFormFile attachment, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isCompanyMode = IsCompanyMode();
        int? companyId = null;

        if (isCompanyMode)
        {
            companyId = GetCompanyId();
            var chatExists = await VerifyChatAuthorizationAsync(id, companyId.Value, cancellationToken);
            if (!chatExists)
            {
                return Forbid();
            }
        }

        if (attachment is null || attachment.Length == 0)
        {
            TempData["ChatError"] = "No file selected.";
            return RedirectToAction(nameof(Show), new { id });
        }

        try
        {
            await using var stream = attachment.OpenReadStream();
            var path = await fileStorage.SaveFileAsync(stream, attachment.FileName, cancellationToken);
            var attachementExtension = Path.GetExtension(attachment.FileName);
            var type = ImageExtensions.Contains(attachementExtension) ? MessageType.Image : MessageType.File;
            await chat.SendStoredAttachmentAsync(id, path, attachment.FileName, userId, type, companyId, cancellationToken);
        }
        catch (Exception exception)
        {
            TempData["ChatError"] = exception.Message;
        }

        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isCompanyMode = IsCompanyMode();
        int? companyId = null;

        if (isCompanyMode)
        {
            companyId = GetCompanyId();
            var chatExists = await VerifyChatAuthorizationAsync(id, companyId.Value, cancellationToken);
            if (!chatExists)
            {
                return Forbid();
            }
        }

        await chat.DeleteChatAsync(id, userId, companyId, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isCompanyMode = IsCompanyMode();
        int? companyId = null;

        if (isCompanyMode)
        {
            companyId = GetCompanyId();
            var chatExists = await VerifyChatAuthorizationAsync(id, companyId.Value, cancellationToken);
            if (!chatExists)
            {
                return Forbid();
            }
        }

        await chat.BlockChatAsync(id, userId, companyId, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isCompanyMode = IsCompanyMode();
        int? companyId = null;

        if (isCompanyMode)
        {
            companyId = GetCompanyId();
            var chatExists = await VerifyChatAuthorizationAsync(id, companyId.Value, cancellationToken);
            if (!chatExists)
            {
                return Forbid();
            }
        }

        await chat.UnblockChatAsync(id, userId, companyId, cancellationToken);
        return RedirectToAction(nameof(Show), new { id });
    }

    private int GetUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idValue))
        {
            throw new InvalidOperationException("User session is missing a user id.");
        }

        return int.Parse(idValue, System.Globalization.CultureInfo.InvariantCulture);
    }

    private int GetCompanyId()
    {
        var companyIdValue = User.FindFirstValue("CompanyId");
        if (!string.IsNullOrWhiteSpace(companyIdValue) && int.TryParse(companyIdValue, System.Globalization.CultureInfo.InvariantCulture, out var companyId))
        {
            return companyId;
        }

        // Fallback for backwards compatibility
        return apiConfiguration.TemporaryCompanyId;
    }

    private int GetCallerId()
    {
        // Messages should always be sent by the actual user, regardless of mode
        // Company mode only determines which chats are accessible
        return GetUserId();
    }

    private async Task<bool> VerifyChatAuthorizationAsync(int chatId, int companyId, CancellationToken cancellationToken)
    {
        // Get all company chats and verify the requested chat belongs to this company
        var companyChats = await chat.GetChatsForCompanyAsync(companyId, cancellationToken);
        return companyChats.Any(c => c.ChatId == chatId);
    }

    private bool IsCompanyMode()
        => string.Equals(HttpContext.Session.GetString(SessionKeys.Mode), AppModes.Company, StringComparison.OrdinalIgnoreCase);
}
