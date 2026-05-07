using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Messages;

public interface IMessageRepository
{
    Task<IReadOnlyList<Message>> GetForChatAsync(int chatId, CancellationToken ct = default);

    Task<Message?> GetLatestForChatAsync(int chatId, CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(int chatId, int senderId, CancellationToken ct = default);

    Task<Message> AddAsync(Message message, CancellationToken ct = default);

    Task MarkAsReadAsync(int chatId, int readerId, CancellationToken ct = default);
}
