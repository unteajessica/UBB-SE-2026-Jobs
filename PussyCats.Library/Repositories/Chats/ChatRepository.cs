using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Chats;

public class ChatRepository : IChatRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public ChatRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    public async Task<Chat?> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Chats
            .AsNoTracking()
            .Include(chat => chat.SecondUser)
            .FirstOrDefaultAsync(chat => chat.ChatId == chatId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Chat>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Chats
            .AsNoTracking()
            .Include(chat => chat.SecondUser)
            .Where(chat => chat.UserId == userId || chat.SecondUserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Chat>> GetForCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Chats
            .AsNoTracking()
            .Include(chat => chat.SecondUser)
            .Where(chat => chat.CompanyId == companyId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Chat?> FindUserUserChatAsync(int userId, int secondUserId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Chats
            .AsNoTracking()
            .Include(chat => chat.SecondUser)
            .FirstOrDefaultAsync(
                chat => (chat.UserId == userId && chat.SecondUserId == secondUserId)
                     || (chat.UserId == secondUserId && chat.SecondUserId == userId),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Chat?> FindUserCompanyChatAsync(int userId, int companyId, int? jobId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Chats
            .AsNoTracking()
            .Include(chat => chat.SecondUser)
            .FirstOrDefaultAsync(
                chat => chat.UserId == userId
                     && chat.CompanyId == companyId
                     && chat.JobId == jobId
                     && chat.SecondUserId == null,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Chat> AddAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        databaseContext.Chats.Add(chat);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return chat;
    }

    public async Task UpdateAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        var tracked = databaseContext.Chats.Local.FirstOrDefault(existing => existing.ChatId == chat.ChatId);
        if (tracked is not null)
        {
            databaseContext.Entry(tracked).CurrentValues.SetValues(chat);
        }
        else
        {
            databaseContext.Entry(chat).State = EntityState.Modified;
        }
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
