using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Chats;

public interface IChatRepository
{
    Task<Chat?> GetByIdAsync(int chatId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Chat>> GetForUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Chat>> GetForCompanyAsync(int companyId, CancellationToken cancellationToken = default);

    Task<Chat?> FindUserUserChatAsync(int userId, int secondUserId, CancellationToken cancellationToken = default);

    Task<Chat?> FindUserCompanyChatAsync(int userId, Company company, int? jobId,
        CancellationToken cancellationToken = default);

    Task<Chat> AddAsync(Chat chat, CancellationToken cancellationToken = default);

    Task UpdateAsync(Chat chat, CancellationToken cancellationToken = default);
}
