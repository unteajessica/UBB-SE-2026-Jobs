using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Messages;

namespace PussyCats.App.RepositoryProxies;

public class MessageRepositoryProxy : IMessageRepository
{
    private readonly HttpClient http;

    public MessageRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<Message>> GetForChatAsync(int chatId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<Message>(http, $"api/chats/{chatId}/messages", ct).ConfigureAwait(false);
    }

    public async Task<Message?> GetLatestForChatAsync(int chatId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Message>(http, $"api/chats/{chatId}/messages/latest", ct).ConfigureAwait(false);
    }

    public async Task<int> GetUnreadCountAsync(int chatId, int senderId, CancellationToken ct = default)
    {
        var result = await RepositoryProxyJson.GetOrNullAsync<int>(http, $"api/chats/{chatId}/messages/unread?senderId={senderId}", ct).ConfigureAwait(false);
        return result;
    }

    public async Task<Message> AddAsync(Message message, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync(
            $"api/chats/{message.ChatId}/messages",
            new { message.SenderId, message.Content, message.Type, OriginalFileName = string.IsNullOrEmpty(message.OriginalFileName) ? null : message.OriginalFileName },
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Message>(response, ct).ConfigureAwait(false);
    }

    public async Task MarkAsReadAsync(int chatId, int readerId, CancellationToken ct = default)
    {
        using var response = await http.PostAsync($"api/chats/{chatId}/messages/read?readerId={readerId}", null, ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
