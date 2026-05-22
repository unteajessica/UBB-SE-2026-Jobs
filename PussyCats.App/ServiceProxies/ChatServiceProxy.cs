using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.ChatService;

namespace PussyCats.App.ServiceProxies;

public sealed class ChatServiceProxy : IChatService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient http;

    public ChatServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<Chat?> FindOrCreateUserCompanyChatAsync(int userId, Company company, Job? job = null, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync(
            "api/chats",
            new { UserId = userId, Company = company, Job = job, SecondUserId = (int?)null },
            JsonOptions,
            cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Chat>(JsonOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<Chat?> FindOrCreateUserChatAsync(int userId, int secondUserId, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync(
            "api/chats",
            new { UserId = userId, SecondUserId = secondUserId, Company = (Company?)null, Job = (Job?)null },
            JsonOptions,
            cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Chat>(JsonOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Chat>> GetChatsForUserAsync(int userId, CancellationToken cancellationToken = default)
        => await GetChatsAsync($"api/chats?userId={userId}&callerId={userId}", cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Chat>> GetChatsForCompanyAsync(int companyId, CancellationToken cancellationToken = default)
        => await GetChatsAsync($"api/chats?companyId={companyId}&callerId={companyId}", cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Message>> GetMessagesAsync(int chatId, int callerId, CancellationToken cancellationToken = default)
        => await GetListAsync<Message>($"api/chats/{chatId}/messages?callerId={callerId}", cancellationToken).ConfigureAwait(false);

    public async Task SendMessageAsync(int chatId, string content, int senderId, MessageType typeOfMessage, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync(
            $"api/chats/{chatId}/messages",
            new { SenderId = senderId, Content = content, Type = typeOfMessage },
            JsonOptions,
            cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }

    public async Task SendStoredAttachmentAsync(int chatId, string storedPath, string originalFileName, int senderId, MessageType type, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync(
            $"api/chats/{chatId}/attachments",
            new { SenderId = senderId, StoredPath = storedPath, OriginalFileName = originalFileName, Type = type },
            JsonOptions,
            cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }

    public async Task<Stream> OpenMessageAttachmentAsync(string attachmentPath, CancellationToken cancellationToken = default)
        => await DownloadFileAsync(attachmentPath, cancellationToken).ConfigureAwait(false);

    public async Task MarkMessagesAsReadAsync(int chatId, int readerId, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsync($"api/chats/{chatId}/messages/read?readerId={readerId}", null, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task BlockChatAsync(int chatId, int blockerId, CancellationToken cancellationToken = default)
    {
        using var response = await http.PatchAsync(
            $"api/chats/{chatId}/block",
            JsonContent.Create(new { CallerId = blockerId }, options: JsonOptions),
            cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }

    public async Task UnblockChatAsync(int chatId, int unblockerId, CancellationToken cancellationToken = default)
    {
        using var response = await http.PatchAsync(
            $"api/chats/{chatId}/unblock",
            JsonContent.Create(new { CallerId = unblockerId }, options: JsonOptions),
            cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteChatAsync(int chatId, int callerId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/chats/{chatId}?callerId={callerId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<Company>> SearchCompaniesAsync(string companyNameSearchTerm, CancellationToken cancellationToken = default)
        => await GetListAsync<Company>($"api/chats/search/companies?q={Uri.EscapeDataString(companyNameSearchTerm)}", cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<User>> SearchUsersAsync(string userNameSearchTerm, CancellationToken cancellationToken = default)
        => await GetListAsync<User>($"api/chats/search/users?q={Uri.EscapeDataString(userNameSearchTerm)}", cancellationToken)
            .ConfigureAwait(false);

    private async Task<IReadOnlyList<Chat>> GetChatsAsync(string path, CancellationToken cancellationToken)
        => await GetListAsync<Chat>(path, cancellationToken).ConfigureAwait(false);

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string path, CancellationToken cancellationToken)
    {
        var response = await http.GetAsync(path, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyList<T>>(JsonOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
        return payload ?? Array.Empty<T>();
    }

    private async Task<Stream> DownloadFileAsync(string relativePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("File path cannot be empty.", nameof(relativePath));
        }

        var fileName = Uri.EscapeDataString(Path.GetFileName(relativePath));
        var response = await http.GetAsync($"api/files/{fileName}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var memory = new MemoryStream();
        await response.Content.CopyToAsync(memory, cancellationToken).ConfigureAwait(false);
        memory.Position = 0;
        return memory;
    }
}
