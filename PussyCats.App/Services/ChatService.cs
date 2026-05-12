using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Chats;
using PussyCats.Library.Repositories.Messages;

namespace PussyCats.App.Services;

public sealed class ChatService : IChatService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
    };

    private static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".docx",
        ".doc",
    };

    private const long MaxImageBytes = 10 * 1024 * 1024;
    private const long MaxFileBytes = 20 * 1024 * 1024;
    private const int MaxSearchResults = 10;
    private const int MaxTextMessageLength = 2000;

    private readonly IChatRepository chatRepository;
    private readonly IMessageRepository messageRepository;
    private readonly IUserService userService;
    private readonly ICompanyService companyService;
    private readonly ILocalFileStorageService fileStorage;

    public ChatService(
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        IUserService userService,
        ICompanyService companyService,
        ILocalFileStorageService fileStorage)
    {
        this.chatRepository = chatRepository;
        this.messageRepository = messageRepository;
        this.userService = userService;
        this.companyService = companyService;
        this.fileStorage = fileStorage;
    }

    public async Task<Chat?> FindOrCreateUserCompanyChatAsync(int userId, Company company, Job? job = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await chatRepository.FindUserCompanyChatAsync(userId, company, job?.JobId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            existing.DeletedAtByUser = null;
            existing.DeletedAtBySecondParty = null;
            await chatRepository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            return existing;
        }

        return await chatRepository.AddAsync(new Chat { User = await GetUserAsync(userId, cancellationToken), Company = company, Job = job }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Chat?> FindOrCreateUserChatAsync(int userId, int secondUserId, CancellationToken cancellationToken = default)
    {
        var existing = await chatRepository.FindUserUserChatAsync(userId, secondUserId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            existing.DeletedAtByUser = null;
            existing.DeletedAtBySecondParty = null;
            await chatRepository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            return existing;
        }

        return await chatRepository.AddAsync(new Chat { User = await GetUserAsync(userId, cancellationToken), SecondUserId = secondUserId }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Chat>> GetChatsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var chats = await chatRepository.GetForUserAsync(userId, cancellationToken).ConfigureAwait(false);
        return chats.Where(chat => ShouldIncludeChat(chat, userId)).ToList();
    }

    public async Task<IReadOnlyList<Chat>> GetChatsForCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var chats = await chatRepository.GetForCompanyAsync(companyId, cancellationToken).ConfigureAwait(false);
        return chats.Where(chat => ShouldIncludeChat(chat, companyId)).ToList();
    }

    public async Task<IReadOnlyList<Message>> GetMessagesAsync(int chatId, int callerId, CancellationToken cancellationToken = default)
    {
        var chat = await chatRepository.GetByIdAsync(chatId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Chat {chatId} not found.");
        EnsureParticipant(chat, callerId);

        var deletedAt = chat.User.UserId == callerId ? chat.DeletedAtByUser : chat.DeletedAtBySecondParty;
        var messages = await messageRepository.GetForChatAsync(chatId, cancellationToken).ConfigureAwait(false);

        return messages
            .Where(message => deletedAt is null || message.Timestamp > deletedAt)
            .Select(message => WithPresentationFields(message, callerId))
            .ToList();
    }

    public async Task<IReadOnlyList<Company>> SearchCompaniesAsync(string companyNameSearchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(companyNameSearchTerm))
        {
            return Array.Empty<Company>();
        }

        var companies = await companyService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return companies
            .Where(company => company.CompanyName.Contains(companyNameSearchTerm, StringComparison.OrdinalIgnoreCase))
            .Take(MaxSearchResults)
            .ToList();
    }

    public async Task<IReadOnlyList<User>> SearchUsersAsync(string userNameSearchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userNameSearchTerm))
        {
            return Array.Empty<User>();
        }

        var users = await userService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return users
            .Where(user => GetUserName(user).Contains(userNameSearchTerm, StringComparison.OrdinalIgnoreCase))
            .Take(MaxSearchResults)
            .ToList();
    }

    public async Task SendMessageAsync(int chatId, string content, int senderId, MessageType typeOfMessage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be empty.", nameof(content));
        }

        var chat = await chatRepository.GetByIdAsync(chatId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Chat {chatId} not found.");
        EnsureParticipant(chat, senderId);

        if (chat.IsBlocked)
        {
            throw new InvalidOperationException("Cannot send a message in a blocked chat.");
        }

        if (typeOfMessage == MessageType.Text && content.Length > MaxTextMessageLength)
        {
            throw new ArgumentException($"Text messages cannot exceed {MaxTextMessageLength} characters.", nameof(content));
        }

        var originalFileName = string.Empty;
        if (typeOfMessage != MessageType.Text)
        {
            originalFileName = Path.GetFileName(content);
            content = await StoreAttachmentAsync(content, typeOfMessage, cancellationToken).ConfigureAwait(false);
        }

        await messageRepository.AddAsync(new Message
        {
            Chat = new Chat { ChatId = chatId },
            Sender = new MessageSender { SenderId = senderId },
            Content = content.Trim(),
            Timestamp = DateTime.UtcNow,
            Type = typeOfMessage,
            OriginalFileName = originalFileName,
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Stream> OpenMessageAttachmentAsync(string attachmentPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(attachmentPath))
        {
            throw new ArgumentException("Attachment path cannot be empty.", nameof(attachmentPath));
        }

        return await fileStorage.OpenReadAsync(attachmentPath, cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkMessagesAsReadAsync(int chatId, int readerId, CancellationToken cancellationToken = default)
    {
        await messageRepository.MarkAsReadAsync(chatId, readerId, cancellationToken).ConfigureAwait(false);
    }

    public async Task BlockChatAsync(int chatId, int blockerId, CancellationToken cancellationToken = default)
    {
        var chat = await chatRepository.GetByIdAsync(chatId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Chat {chatId} not found.");
        EnsureParticipant(chat, blockerId);
        chat.IsBlocked = true;
        chat.BlockedByUserId = blockerId;
        await chatRepository.UpdateAsync(chat, cancellationToken).ConfigureAwait(false);
    }

    public async Task UnblockChatAsync(int chatId, int unblockerId, CancellationToken cancellationToken = default)
    {
        var chat = await chatRepository.GetByIdAsync(chatId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Chat {chatId} not found.");
        EnsureParticipant(chat, unblockerId);
        if (chat.BlockedByUser?.UserId != unblockerId)
        {
            throw new UnauthorizedAccessException("Only the blocker can unblock this chat.");
        }

        chat.IsBlocked = false;
        chat.BlockedByUserId = null;
        await chatRepository.UpdateAsync(chat, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteChatAsync(int chatId, int callerId, CancellationToken cancellationToken = default)
    {
        var chat = await chatRepository.GetByIdAsync(chatId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Chat {chatId} not found.");
        EnsureParticipant(chat, callerId);

        if (chat.User.UserId == callerId)
        {
            chat.DeletedAtByUser = DateTime.UtcNow;
        }
        else
        {
            chat.DeletedAtBySecondParty = DateTime.UtcNow;
        }

        await chatRepository.UpdateAsync(chat, cancellationToken).ConfigureAwait(false);
    }

    private static bool ShouldIncludeChat(Chat chat, int callerId)
    {
        if (chat.IsBlocked && chat.BlockedByUser?.UserId != callerId)
        {
            return false;
        }

        var deletedAt = chat.User.UserId == callerId ? chat.DeletedAtByUser : chat.DeletedAtBySecondParty;
        return deletedAt is null;
    }

    private static void EnsureParticipant(Chat chat, int callerId)
    {
        if (chat.User.UserId != callerId && chat.SecondUserId != callerId && chat.Company?.CompanyId != callerId)
        {
            throw new UnauthorizedAccessException("Only chat participants can access this chat.");
        }
    }

    private static Message WithPresentationFields(Message message, int callerId)
    {
        return new Message
        {
            MessageId = message.MessageId,
            Chat = new Chat { ChatId = message.Chat.ChatId },
            Sender = new MessageSender { SenderId = message.Sender.SenderId },
            Content = message.Content,
            Timestamp = message.Timestamp,
            Type = message.Type,
            IsRead = message.IsRead,
            OriginalFileName = message.OriginalFileName,
            ShowReadReceipt = message.Sender.SenderId == callerId,
            SenderInitials = message.Sender.SenderId == callerId ? "Me" : "Them",
        };
    }

    private static string GetUserName(User user)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? $"User {user.UserId}" : fullName;
    }

    private static void ValidateAttachment(string path, MessageType type)
    {
        var extension = Path.GetExtension(path);
        if (type == MessageType.Image && !AllowedImageExtensions.Contains(extension))
        {
            throw new NotSupportedException("Image messages must be .jpg, .jpeg, or .png.");
        }

        if (type == MessageType.File && !AllowedFileExtensions.Contains(extension))
        {
            throw new NotSupportedException("File messages must be .pdf, .docx, or .doc.");
        }
    }

    private async Task<User> GetUserAsync(int userId, CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return users.FirstOrDefault(u => u.UserId == userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");
    }

    private async Task<string> StoreAttachmentAsync(string sourcePath, MessageType type, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException("Attachment path cannot be empty.", nameof(sourcePath));
        }

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Attachment file was not found.", sourcePath);
        }

        ValidateAttachment(sourcePath, type);

        var fileInfo = new FileInfo(sourcePath);
        var maxBytes = type == MessageType.Image ? MaxImageBytes : MaxFileBytes;
        if (fileInfo.Length > maxBytes)
        {
            throw new InvalidOperationException(type == MessageType.Image
                ? "Image must be less than 10 MB."
                : "File must be less than 20 MB.");
        }

        await using var stream = File.OpenRead(sourcePath);
        return await fileStorage.SaveFileAsync(stream, Path.GetFileName(sourcePath), cancellationToken)
            .ConfigureAwait(false);
    }
}
