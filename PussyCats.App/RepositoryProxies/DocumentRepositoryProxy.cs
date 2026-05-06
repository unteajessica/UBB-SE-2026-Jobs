using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Documents;

namespace PussyCats.App.RepositoryProxies;

public class DocumentRepositoryProxy : IDocumentRepository
{
    private readonly HttpClient http;

    public DocumentRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<Document?> GetByIdAsync(int documentId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Document>(http, $"api/documents/{documentId}", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<Document>(http, $"api/documents?userId={userId}", ct).ConfigureAwait(false);
    }

    public async Task<Document> AddAsync(Document document, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("api/documents", document, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Document>(response, ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int documentId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/documents/{documentId}", ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
