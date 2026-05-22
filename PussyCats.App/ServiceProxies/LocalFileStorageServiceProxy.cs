using System.Net.Http.Headers;
using System.Net.Http.Json;
using PussyCats.Library.Services.FileStorage;

namespace PussyCats.App.ServiceProxies;

public class FileStorageServiceProxy : ILocalFileStorageService
{
    private readonly HttpClient http;

    public FileStorageServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        using var multipart = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        multipart.Add(streamContent, "file", Path.GetFileName(originalFileName));

        var response = await http.PostAsync("api/files", multipart, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<UploadResponse>(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("File upload returned no body.");
        return payload.Path;
    }

    public async Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var fileName = Uri.EscapeDataString(Path.GetFileName(relativePath));
        var response = await http.DeleteAsync($"api/files/{fileName}", cancellationToken).ConfigureAwait(false);

        if (response.StatusCode is System.Net.HttpStatusCode.NotFound)
            return;

        response.EnsureSuccessStatusCode();
    }

    public async Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("File path cannot be empty.", nameof(relativePath));

        var fileName = Uri.EscapeDataString(Path.GetFileName(relativePath));
        var response = await http.GetAsync($"api/files/{fileName}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var memory = new MemoryStream();
        await response.Content.CopyToAsync(memory, cancellationToken).ConfigureAwait(false);
        memory.Position = 0;
        return memory;
    }

    public string GetUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;

        var baseUrl = http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
        var fileName = Uri.EscapeDataString(Path.GetFileName(relativePath));
        return $"{baseUrl}/api/files/{fileName}";
    }

    private sealed record UploadResponse(string Path);
}
