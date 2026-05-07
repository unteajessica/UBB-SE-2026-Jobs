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

    public async Task<Chat?> GetByIdAsync(int chatId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Chat>(http, $"api/chats/{chatId}", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Chat>> GetForUserAsync(int userId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<Chat>(http, $"api/chats?userId={userId}&callerId={userId}", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Chat>> GetForCompanyAsync(int companyId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<Chat>(http, $"api/chats?companyId={companyId}&callerId={companyId}", ct).ConfigureAwait(false);
    }

    public async Task<Chat?> FindUserUserChatAsync(int userId, int secondUserId, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync(
            "api/chats",
            new { UserId = userId, SecondUserId = secondUserId, CompanyId = (int?)null, JobId = (int?)null },
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Chat>(response, ct).ConfigureAwait(false);
    }

    public async Task<Chat?> FindUserCompanyChatAsync(int userId, int companyId, int? jobId, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync(
            "api/chats",
            new { UserId = userId, CompanyId = companyId, SecondUserId = (int?)null, JobId = jobId },
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Chat>(response, ct).ConfigureAwait(false);
    }

    public async Task<Chat> AddAsync(Chat chat, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync(
            "api/chats",
            new { chat.UserId, chat.CompanyId, chat.SecondUserId, chat.JobId },
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Chat>(response, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Chat chat, CancellationToken ct = default)
    {
        using var response = await http.PutAsJsonAsync($"api/chats/{chat.ChatId}", chat, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
