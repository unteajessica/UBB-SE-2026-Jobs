using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Chats;

public interface IChatRepository
{
    Task<Chat?> GetByIdAsync(int chatId, CancellationToken ct = default);

    Task<IReadOnlyList<Chat>> GetForUserAsync(int userId, CancellationToken ct = default);

    Task<IReadOnlyList<Chat>> GetForCompanyAsync(int companyId, CancellationToken ct = default);

    Task<Chat?> FindUserUserChatAsync(int userId, int secondUserId, CancellationToken ct = default);

    Task<Chat?> FindUserCompanyChatAsync(int userId, int companyId, int? jobId, CancellationToken ct = default);

    Task<Chat> AddAsync(Chat chat, CancellationToken ct = default);

    Task UpdateAsync(Chat chat, CancellationToken ct = default);
}
