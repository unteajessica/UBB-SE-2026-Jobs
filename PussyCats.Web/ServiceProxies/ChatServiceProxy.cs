using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.ChatService;

namespace PussyCats.Web.ServiceProxies;

public class ChatServiceProxy : IChatService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient http;

    public ChatServiceProxy(HttpClient http) => this.http = http;

    public async Task<IReadOnlyList<Chat>> GetChatsForUserAsync(int userId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<Chat>>($"api/chats?userId={userId}&callerId={userId}", JsonOptions, cancellationToken)
           ?? new List<Chat>();

    public async Task<IReadOnlyList<Chat>> GetChatsForCompanyAsync(int companyId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<Chat>>($"api/chats?companyId={companyId}&callerId={companyId}", JsonOptions, cancellationToken)
           ?? new List<Chat>();

    public async Task<IReadOnlyList<Message>> GetMessagesAsync(int chatId, int callerId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<Message>>($"api/chats/{chatId}/messages?callerId={callerId}", JsonOptions, cancellationToken)
           ?? new List<Message>();

    public async Task SendMessageAsync(int chatId, string content, int senderId, MessageType type, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync($"api/chats/{chatId}/messages",
            new { senderId, content, type }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkMessagesAsReadAsync(int chatId, int readerId, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsync($"api/chats/{chatId}/messages/read?readerId={readerId}", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task BlockChatAsync(int chatId, int blockerId, CancellationToken cancellationToken = default)
    {
        var response = await http.PatchAsJsonAsync($"api/chats/{chatId}/block", new { callerId = blockerId }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UnblockChatAsync(int chatId, int unblockerId, CancellationToken cancellationToken = default)
    {
        var response = await http.PatchAsJsonAsync($"api/chats/{chatId}/unblock", new { callerId = unblockerId }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteChatAsync(int chatId, int callerId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/chats/{chatId}?callerId={callerId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Chat?> FindOrCreateUserChatAsync(int userId, int secondUserId, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/chats", new { userId, secondUserId, company = (object?)null, job = (object?)null }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Chat>(JsonOptions, cancellationToken: cancellationToken);
    }

    public async Task<Chat?> FindOrCreateUserCompanyChatAsync(int userId, Company company, Job? job = null, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/chats", new { userId, secondUserId = (int?)null, company, job }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Chat>(JsonOptions, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<User>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<User>>($"api/chats/search/users?q={Uri.EscapeDataString(query)}", JsonOptions, cancellationToken)
           ?? new List<User>();

    public async Task<IReadOnlyList<Company>> SearchCompaniesAsync(string query, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<Company>>($"api/chats/search/companies?q={Uri.EscapeDataString(query)}", JsonOptions, cancellationToken)
           ?? new List<Company>();

    public async Task SendStoredAttachmentAsync(int chatId, string storedPath, string originalFileName, int senderId, MessageType type, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync($"api/chats/{chatId}/attachments",
            new { senderId, storedPath, originalFileName, type }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task<Stream> OpenMessageAttachmentAsync(string attachmentPath, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("File attachments are not supported in the web client.");
}
