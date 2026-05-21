using System.Net.Http.Json;
using System.Net.Http.Headers;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Documents;

namespace PussyCats.Web.ServiceProxies;

public class DocumentServiceProxy : IDocumentService
{
    private readonly HttpClient http;

    public DocumentServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<Document>>("api/documents", cancellationToken) ?? new List<Document>();

    public async Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/documents/{documentId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Document>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> GetDocumentsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<Document>>($"api/documents?userId={userId}", cancellationToken) ?? new List<Document>();

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        var request = new DocumentAddRequest
        {
            UserId = document.User.UserId,
            DocumentName = document.DocumentName,
            FilePath = document.FilePath,
        };
        var response = await http.PostAsJsonAsync("api/documents", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Document>(cancellationToken: cancellationToken))!;
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        var request = new DocumentUpdateRequest
        {
            DocumentName = document.DocumentName,
            FilePath = document.FilePath,
        };
        var response = await http.PutAsJsonAsync($"api/documents/{document.DocumentId}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/documents/{documentId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Document> UploadDocumentAsync(Document document, string filePath, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("File upload is not supported via the HTTP proxy.");

    public async Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("File deletion is not supported via the HTTP proxy.");

    public async Task<Document> UploadDocumentFromStreamAsync(
        int userId,
        string documentName,
        string originalFileName,
        string contentType,
        Stream fileStream,
        bool isCv,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(userId.ToString()), "UserId");
        content.Add(new StringContent(documentName), "DocumentName");
        content.Add(new StringContent(isCv.ToString()), "IsCv");

        using var streamContent = new StreamContent(fileStream);
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }

        content.Add(streamContent, "File", originalFileName);

        var response = await http.PostAsync("api/documents/upload", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Document>(cancellationToken: cancellationToken))!;
    }

    public async Task<string> GetDocumentAbsolutePathAsync(int documentId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Path retrieval is not supported via the HTTP proxy.");
}

