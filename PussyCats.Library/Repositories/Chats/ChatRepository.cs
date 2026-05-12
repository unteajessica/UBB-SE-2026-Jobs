using System.Diagnostics;
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
            .Include(chat => chat.User)
            .Include(chat => chat.SecondUser)
            .Include(chat => chat.Company)
            .AsNoTracking()
            .Include(chat => chat.BlockedByUser)
            .FirstOrDefaultAsync(chat => chat.ChatId == chatId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Chat>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        System.Diagnostics.Debug.WriteLine("HEREEEEE "+userId);
        return await databaseContext.Chats
            .Include(chat => chat.User)
            .Include(chat => chat.SecondUser)
            .Include(chat => chat.Company)
            .AsNoTracking()
            .Include(chat => chat.BlockedByUser)
            .Where(chat => chat.User.UserId == userId || chat.SecondUser.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Chat>> GetForCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Chats
            .Include(chat => chat.User)
            .Include(chat => chat.SecondUser)
            .AsNoTracking()
            .Include(chat => chat.BlockedByUser)
            .Where(chat => chat.Company!=null && chat.Company.CompanyId == companyId)
            .Include(chat => chat.Company)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Chat?> FindUserUserChatAsync(int userId, int secondUserId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Chats
            .Include(chat => chat.User)
            .Include(chat => chat.SecondUser)
            .AsNoTracking()
            .Include(chat => chat.BlockedByUser)
            .FirstOrDefaultAsync(
                chat => (chat.User.UserId == userId && chat.SecondUser.UserId == secondUserId)
                     || (chat.User.UserId == secondUserId && chat.SecondUser.UserId == userId),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Chat?> FindUserCompanyChatAsync(int userId, Company company, int? jobId,
        CancellationToken cancellationToken = default)
    {
        return await databaseContext.Chats
            .Include(chat => chat.User)
            .Include(chat => chat.SecondUser)
            .Include(chat => chat.Company)
            .AsNoTracking()
            .Include(chat => chat.BlockedByUser)
            .FirstOrDefaultAsync(
                chat => chat.User.UserId == userId
                        && chat.Company != null 
                        && chat.Company.CompanyId == company.CompanyId
                        && chat.Job!=null && chat.Job.JobId == jobId
                        && chat.SecondUser.UserId == null,
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
