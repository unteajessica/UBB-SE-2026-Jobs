using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats_App.Services.ChatService;

public interface IChatService
{
    Task<Chat?> FindOrCreateUserCompanyChatAsync(int userId, Company company, Job? job = null,
        CancellationToken cancellationToken = default);

    Task<Chat?> FindOrCreateUserChatAsync(int userId, int secondUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Chat>> GetChatsForUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Chat>> GetChatsForCompanyAsync(int companyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Message>> GetMessagesAsync(int chatId, int callerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Company>> SearchCompaniesAsync(string query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> SearchUsersAsync(string query, CancellationToken cancellationToken = default);

    Task SendMessageAsync(int chatId, string content, int senderId, MessageType type, CancellationToken cancellationToken = default);

    Task<Stream> OpenMessageAttachmentAsync(string attachmentPath, CancellationToken cancellationToken = default);

    Task MarkMessagesAsReadAsync(int chatId, int readerId, CancellationToken cancellationToken = default);

    Task BlockChatAsync(int chatId, int blockerId, CancellationToken cancellationToken = default);

    Task UnblockChatAsync(int chatId, int unblockerId, CancellationToken cancellationToken = default);

    Task DeleteChatAsync(int chatId, int callerId, CancellationToken cancellationToken = default);
}
