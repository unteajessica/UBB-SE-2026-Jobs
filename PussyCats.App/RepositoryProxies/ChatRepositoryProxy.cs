using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Chats;

namespace PussyCats.App.RepositoryProxies;

public class ChatRepositoryProxy : IChatRepository
{
    private readonly HttpClient http;

    public ChatRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<Chat?> GetByIdAsync(int chatId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Chat>(http, $"api/chats/{chatId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Chat>> GetForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<Chat>(http, $"api/chats?userId={userId}&callerId={userId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Chat>> GetForCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<Chat>(http, $"api/chats?companyId={companyId}&callerId={companyId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<Chat?> FindUserUserChatAsync(int userId, int secondUserId, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync(
            "api/chats",
            new { UserId = userId, SecondUserId = secondUserId, CompanyId = (int?)null, JobId = (int?)null },
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Chat>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Chat?> FindUserCompanyChatAsync(int userId, Company company, int? jobId,
        CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync(
            "api/chats",
            new { UserId = userId, Company = company, SecondUserId = (int?)null, JobId = jobId },
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Chat>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Chat> AddAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync(
            "api/chats",
            new { UserId = chat.User.UserId, chat.Company, SecondUserId = chat.SecondUser.UserId, chat.Job },
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Chat>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"api/chats/{chat.ChatId}", chat, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
