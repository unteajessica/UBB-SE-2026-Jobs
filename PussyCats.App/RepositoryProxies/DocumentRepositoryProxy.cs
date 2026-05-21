using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Library.Repositories.Documents;

namespace PussyCats.App.RepositoryProxies;

public class DocumentRepositoryProxy : IDocumentRepository
{
    private readonly HttpClient http;

    public DocumentRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<Document>(http, "api/documents", cancellationToken).ConfigureAwait(false);
    }

    public async Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Document>(http, $"api/documents/{documentId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<Document>(http, $"api/documents?userId={userId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        var request = new DocumentAddRequest
        {
            UserId = document.User.UserId,
            DocumentName = document.DocumentName,
            FilePath = document.FilePath,
        };
        using var response = await http.PostAsJsonAsync("api/documents", request, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Document>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int documentId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/documents/{documentId}", cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        var request = new DocumentUpdateRequest
        {
            DocumentName = document.DocumentName,
            FilePath = document.FilePath,
        };
        using var response = await http.PutAsJsonAsync($"api/documents/{document.DocumentId}", request, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}

