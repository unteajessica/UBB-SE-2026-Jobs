using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Messages;

public interface IMessageRepository
{
    Task<IReadOnlyList<Message>> GetForChatAsync(int chatId, CancellationToken cancellationToken = default);

    Task<Message?> GetLatestForChatAsync(int chatId, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(int chatId, int senderId, CancellationToken cancellationToken = default);

    Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(int chatId, int readerId, CancellationToken cancellationToken = default);
}
