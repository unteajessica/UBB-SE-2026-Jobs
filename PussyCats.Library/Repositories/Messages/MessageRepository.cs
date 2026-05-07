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

    public async Task<IReadOnlyList<Message>> GetForChatAsync(int chatId, CancellationToken ct = default)
    {
        return await databaseContext.Messages
            .AsNoTracking()
            .Where(message => message.ChatId == chatId)
            .OrderBy(message => message.Timestamp)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<Message?> GetLatestForChatAsync(int chatId, CancellationToken ct = default)
    {
        return await databaseContext.Messages
            .AsNoTracking()
            .Where(message => message.ChatId == chatId)
            .OrderBy(message => message.Timestamp)
            .LastOrDefaultAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<int> GetUnreadCountAsync(int chatId, int senderId, CancellationToken ct = default)
    {
        return await databaseContext.Messages
            .CountAsync(
                message => message.ChatId == chatId
                        && message.SenderId != senderId
                        && !message.IsRead,
                ct)
            .ConfigureAwait(false);
    }

    public async Task<Message> AddAsync(Message message, CancellationToken ct = default)
    {
        databaseContext.Messages.Add(message);
        await databaseContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return message;
    }

    public async Task MarkAsReadAsync(int chatId, int readerId, CancellationToken ct = default)
    {
        await databaseContext.Messages
            .Where(message => message.ChatId == chatId && message.SenderId != readerId && !message.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(message => message.IsRead, true), ct)
            .ConfigureAwait(false);
    }
}
