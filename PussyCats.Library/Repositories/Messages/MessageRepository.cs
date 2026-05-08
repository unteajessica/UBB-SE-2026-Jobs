using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Messages;

public class MessageRepository : IMessageRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public MessageRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    public async Task<IReadOnlyList<Message>> GetForChatAsync(int chatId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Messages
            .AsNoTracking()
            .Where(message => message.ChatId == chatId)
            .OrderBy(message => message.Timestamp)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Message?> GetLatestForChatAsync(int chatId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Messages
            .AsNoTracking()
            .Where(message => message.ChatId == chatId)
            .OrderBy(message => message.Timestamp)
            .LastOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> GetUnreadCountAsync(int chatId, int senderId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Messages
            .CountAsync(
                message => message.ChatId == chatId
                        && message.SenderId != senderId
                        && !message.IsRead,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        databaseContext.Messages.Add(message);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return message;
    }

    public async Task MarkAsReadAsync(int chatId, int readerId, CancellationToken cancellationToken = default)
    {
        await databaseContext.Messages
            .Where(message => message.ChatId == chatId && message.SenderId != readerId && !message.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(message => message.IsRead, true), cancellationToken)
            .ConfigureAwait(false);
    }
}
