using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

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

    private readonly IUserService userService;
    private readonly ICompanyService companyService;
    private readonly IJobService jobService;
    private readonly List<Chat> chats = new();
    private readonly List<Message> messages = new();
    private readonly Dictionary<int, string> userNames = new();
    private readonly Dictionary<int, string> companyNames = new();
    private int nextChatId = 1;
    private int nextMessageId = 1;
    private bool seeded;

    public ChatService(IUserService userService, ICompanyService companyService, IJobService jobService)
    {
        this.userService = userService;
        this.companyService = companyService;
        this.jobService = jobService;
    }

    public async Task<Chat?> FindOrCreateUserCompanyChatAsync(int userId, int companyId, int? jobId = null, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);
        var existing = chats.FirstOrDefault(chat =>
            chat.UserId == userId && chat.CompanyId == companyId && chat.JobId == jobId && chat.SecondUserId is null);
        if (existing is not null)
        {
            existing.DeletedAtByUser = null;
            existing.DeletedAtBySecondParty = null;
            return existing;
        }

        var chat = new Chat
        {
            ChatId = nextChatId++,
            UserId = userId,
            CompanyId = companyId,
            JobId = jobId,
        };
        chats.Add(chat);
        return chat;
    }

    public async Task<Chat?> FindOrCreateUserChatAsync(int userId, int secondUserId, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);
        var existing = chats.FirstOrDefault(chat =>
            (chat.UserId == userId && chat.SecondUserId == secondUserId)
            || (chat.UserId == secondUserId && chat.SecondUserId == userId));
        if (existing is not null)
        {
            existing.DeletedAtByUser = null;
            existing.DeletedAtBySecondParty = null;
            return existing;
        }

        var chat = new Chat
        {
            ChatId = nextChatId++,
            UserId = userId,
            SecondUserId = secondUserId,
        };
        chats.Add(chat);
        return chat;
    }

    public async Task<IReadOnlyList<Chat>> GetChatsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);
        return chats
            .Where(chat => chat.UserId == userId || chat.SecondUserId == userId)
            .Where(chat => ShouldIncludeChat(chat, userId))
            .OrderByDescending(ResolveLatestMessageTime)
            .Select(chat => CloneWithPreview(chat, userId))
            .ToList();
    }

    public async Task<IReadOnlyList<Chat>> GetChatsForCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);
        return chats
            .Where(chat => chat.CompanyId == companyId)
            .Where(chat => ShouldIncludeChat(chat, companyId))
            .OrderByDescending(ResolveLatestMessageTime)
            .Select(chat => CloneWithPreview(chat, companyId))
            .ToList();
    }

    public async Task<IReadOnlyList<Message>> GetMessagesAsync(int chatId, int callerId, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);
        var chat = FindChat(chatId);
        EnsureParticipant(chat, callerId);
        var deletedAt = chat.UserId == callerId ? chat.DeletedAtByUser : chat.DeletedAtBySecondParty;
        return messages
            .Where(message => message.ChatId == chatId)
            .Where(message => deletedAt is null || message.Timestamp > deletedAt)
            .OrderBy(message => message.Timestamp)
            .Select(message => CloneMessage(message, callerId))
            .ToList();
    }

    public async Task<IReadOnlyList<Company>> SearchCompaniesAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<Company>();
        }

        var companies = await companyService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        foreach (var company in companies)
        {
            companyNames[company.CompanyId] = company.CompanyName;
        }

        return companies
            .Where(company => company.CompanyName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();
    }

    public async Task<IReadOnlyList<User>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<User>();
        }

        var users = await userService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        foreach (var user in users)
        {
            userNames[user.UserId] = GetUserName(user);
        }

        return users
            .Where(user => GetUserName(user).Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();
    }

    public async Task SendMessageAsync(int chatId, string content, int senderId, MessageType type, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be empty.", nameof(content));
        }

        var chat = FindChat(chatId);
        EnsureParticipant(chat, senderId);
        if (chat.IsBlocked)
        {
            throw new InvalidOperationException("Cannot send a message in a blocked chat.");
        }

        if (type == MessageType.Text && content.Length > 2000)
        {
            throw new ArgumentException("Text messages cannot exceed 2000 characters.", nameof(content));
        }

        if (type != MessageType.Text)
        {
            ValidateAttachment(content, type);
        }

        messages.Add(new Message
        {
            MessageId = nextMessageId++,
            ChatId = chatId,
            SenderId = senderId,
            Content = content.Trim(),
            Timestamp = DateTime.UtcNow,
            Type = type,
        });
    }

    public async Task MarkMessagesAsReadAsync(int chatId, int readerId, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);
        var chat = FindChat(chatId);
        EnsureParticipant(chat, readerId);
        foreach (var message in messages.Where(message => message.ChatId == chatId && message.SenderId != readerId))
        {
            message.IsRead = true;
        }
    }

    public async Task BlockChatAsync(int chatId, int blockerId, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);
        var chat = FindChat(chatId);
        EnsureParticipant(chat, blockerId);
        chat.IsBlocked = true;
        chat.BlockedByUserId = blockerId;
    }

    public async Task UnblockChatAsync(int chatId, int unblockerId, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);
        var chat = FindChat(chatId);
        EnsureParticipant(chat, unblockerId);
        if (chat.BlockedByUserId != unblockerId)
        {
            throw new UnauthorizedAccessException("Only the blocker can unblock this chat.");
        }

        chat.IsBlocked = false;
        chat.BlockedByUserId = null;
    }

    public async Task DeleteChatAsync(int chatId, int callerId, CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken).ConfigureAwait(false);
        var chat = FindChat(chatId);
        EnsureParticipant(chat, callerId);
        if (chat.UserId == callerId)
        {
            chat.DeletedAtByUser = DateTime.UtcNow;
        }
        else
        {
            chat.DeletedAtBySecondParty = DateTime.UtcNow;
        }
    }

    private async Task EnsureSeededAsync(CancellationToken cancellationToken)
    {
        if (seeded)
        {
            return;
        }

        seeded = true;
        var users = await userService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var companies = await companyService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var jobs = await jobService.GetAllAsync(cancellationToken).ConfigureAwait(false);
        foreach (var knownUser in users)
        {
            userNames[knownUser.UserId] = GetUserName(knownUser);
        }

        foreach (var knownCompany in companies)
        {
            companyNames[knownCompany.CompanyId] = knownCompany.CompanyName;
        }

        var user = users.FirstOrDefault(user => user.UserId == 1) ?? users.FirstOrDefault();
        var secondUser = users.FirstOrDefault(candidate => user is not null && candidate.UserId != user.UserId);
        var company = companies.FirstOrDefault(company => company.CompanyId == 1) ?? companies.FirstOrDefault();
        var job = company is null ? jobs.FirstOrDefault() : jobs.FirstOrDefault(job => job.CompanyId == company.CompanyId);

        if (user is null)
        {
            return;
        }

        if (company is not null)
        {
            var chat = new Chat
            {
                ChatId = nextChatId++,
                UserId = user.UserId,
                CompanyId = company.CompanyId,
                JobId = job?.JobId,
            };
            chats.Add(chat);
            AddSeedMessage(chat.ChatId, company.CompanyId, "Thanks for applying. We liked your profile and would like to schedule a short call.", -2);
            AddSeedMessage(chat.ChatId, user.UserId, "Sounds great. I am available tomorrow afternoon.", -1);
        }

        if (secondUser is not null)
        {
            var chat = new Chat
            {
                ChatId = nextChatId++,
                UserId = user.UserId,
                SecondUserId = secondUser.UserId,
            };
            chats.Add(chat);
            AddSeedMessage(chat.ChatId, secondUser.UserId, "Good luck with the interviews. The SQL test helped me a lot.", -3);
        }
    }

    private void AddSeedMessage(int chatId, int senderId, string content, int hoursOffset)
    {
        messages.Add(new Message
        {
            MessageId = nextMessageId++,
            ChatId = chatId,
            SenderId = senderId,
            Content = content,
            Timestamp = DateTime.UtcNow.AddHours(hoursOffset),
            Type = MessageType.Text,
            IsRead = true,
        });
    }

    private Chat FindChat(int chatId)
    {
        return chats.FirstOrDefault(chat => chat.ChatId == chatId)
            ?? throw new KeyNotFoundException($"Chat with id {chatId} was not found.");
    }

    private static void EnsureParticipant(Chat chat, int callerId)
    {
        if (chat.UserId != callerId && chat.SecondUserId != callerId && chat.CompanyId != callerId)
        {
            throw new UnauthorizedAccessException("Only chat participants can access this chat.");
        }
    }

    private bool ShouldIncludeChat(Chat chat, int callerId)
    {
        if (chat.IsBlocked && chat.BlockedByUserId != callerId)
        {
            return false;
        }

        var deletedAt = chat.UserId == callerId ? chat.DeletedAtByUser : chat.DeletedAtBySecondParty;
        return deletedAt is null || ResolveLatestMessageTime(chat) > deletedAt;
    }

    private DateTime ResolveLatestMessageTime(Chat chat)
    {
        return messages
            .Where(message => message.ChatId == chat.ChatId)
            .Select(message => message.Timestamp)
            .DefaultIfEmpty(DateTime.MinValue)
            .Max();
    }

    private Chat CloneWithPreview(Chat chat, int callerId)
    {
        var clone = new Chat
        {
            ChatId = chat.ChatId,
            UserId = chat.UserId,
            CompanyId = chat.CompanyId,
            SecondUserId = chat.SecondUserId,
            JobId = chat.JobId,
            IsBlocked = chat.IsBlocked,
            BlockedByUserId = chat.BlockedByUserId,
            DeletedAtByUser = chat.DeletedAtByUser,
            DeletedAtBySecondParty = chat.DeletedAtBySecondParty,
            OtherPartyName = ResolveOtherPartyName(chat, callerId),
        };

        var chatMessages = messages
            .Where(message => message.ChatId == chat.ChatId)
            .OrderBy(message => message.Timestamp)
            .ToList();
        var lastMessage = chatMessages.LastOrDefault();
        if (lastMessage is null)
        {
            return clone;
        }

        clone.LastMessage = GetDisplayContent(lastMessage);
        clone.LastMessageSnippet = clone.LastMessage.Length > 60 ? $"{clone.LastMessage[..57]}..." : clone.LastMessage;
        clone.LastMessageTime = lastMessage.Timestamp.ToLocalTime().Date == DateTime.Now.Date
            ? lastMessage.Timestamp.ToLocalTime().ToString("HH:mm")
            : lastMessage.Timestamp.ToLocalTime().ToString("dd MMM");
        clone.UnreadCount = chatMessages.Count(message => message.SenderId != callerId && !message.IsRead);
        return clone;
    }

    private Message CloneMessage(Message message, int callerId)
    {
        return new Message
        {
            MessageId = message.MessageId,
            ChatId = message.ChatId,
            SenderId = message.SenderId,
            Content = message.Content,
            Timestamp = message.Timestamp,
            Type = message.Type,
            IsRead = message.IsRead,
            ShowReadReceipt = message.SenderId == callerId,
            SenderInitials = message.SenderId == callerId ? "Me" : "Them",
        };
    }

    private string ResolveOtherPartyName(Chat chat, int callerId)
    {
        if (chat.CompanyId is int companyId && callerId != companyId)
        {
            return companyNames.TryGetValue(companyId, out var companyName) ? companyName : $"Company {companyId}";
        }

        var otherUserId = chat.UserId == callerId ? chat.SecondUserId : chat.UserId;
        if (otherUserId is int userId)
        {
            return userNames.TryGetValue(userId, out var userName) ? userName : $"User {userId}";
        }

        return "Conversation";
    }

    private static string GetUserName(User user)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? $"User {user.UserId}" : fullName;
    }

    private static string GetDisplayContent(Message message)
    {
        return message.Type == MessageType.Text
            ? message.Content
            : Path.GetFileName(message.Content);
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
}
