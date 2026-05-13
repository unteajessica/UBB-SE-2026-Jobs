using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Chats;

namespace PussyCats.Tests.Fakes
{

    public class InMemoryChatRepository : IChatRepository
    {
        private readonly List<Chat> chats = new();
        private int nextId = 1;

        public Task<Chat?> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
        {
            var chat = chats.FirstOrDefault(c => c.ChatId == chatId);
            return Task.FromResult(chat);
        }

        public Task<IReadOnlyList<Chat>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var result = chats
                .Where(chat =>
                    chat.User?.UserId == userId ||
                    chat.SecondUser?.UserId == userId)
                .ToList();

            return Task.FromResult((IReadOnlyList<Chat>)result);
        }

        public Task<IReadOnlyList<Chat>> GetForCompanyAsync(int companyId,
            CancellationToken cancellationToken = default)
        {
            var result = chats
                .Where(chat => chat.Company?.CompanyId == companyId)
                .ToList();

            return Task.FromResult((IReadOnlyList<Chat>)result);
        }

        public Task<Chat?> FindUserUserChatAsync(int userId, int secondUserId,
            CancellationToken cancellationToken = default)
        {
            var result = chats.FirstOrDefault(chat =>
                (chat.User?.UserId == userId && chat.SecondUser?.UserId == secondUserId)
                || (chat.User?.UserId == secondUserId && chat.SecondUser?.UserId == userId));

            return Task.FromResult(result);
        }

        public Task<Chat?> FindUserCompanyChatAsync(int userId, Company company, int? jobId,
            CancellationToken cancellationToken = default)
        {
            var result = chats.FirstOrDefault(chat =>
                chat.User?.UserId == userId &&
                chat.Company != null &&
                chat.Company.CompanyId == company.CompanyId &&
                chat.Job != null &&
                chat.Job.JobId == jobId &&
                chat.SecondUser?.UserId == null);

            return Task.FromResult(result);
        }

        public Task<Chat> AddAsync(Chat chat, CancellationToken cancellationToken = default)
        {
            chat.ChatId = nextId++;
            chats.Add(chat);
            return Task.FromResult(chat);
        }

        public Task UpdateAsync(Chat chat, CancellationToken cancellationToken = default)
        {
            var index = chats.FindIndex(c => c.ChatId == chat.ChatId);

            if (index >= 0)
            {
                chats[index] = chat;
            }

            return Task.CompletedTask;
        }

        
        public List<Chat> Dump() => chats;
    }
}