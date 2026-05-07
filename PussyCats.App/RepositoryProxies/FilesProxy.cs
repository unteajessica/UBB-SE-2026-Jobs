using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PussyCats.App.RepositoryProxies;

public class FilesProxy : IFilesProxy
{
    private readonly HttpClient httpClient;

    public FilesProxy(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<string> UploadAsync(Stream content, string originalFileName, CancellationToken cancellationToken = default)
    {
        using var multipart = new MultipartFormDataContent();
        var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        multipart.Add(streamContent, "file", Path.GetFileName(originalFileName));

        using var response = await httpClient.PostAsync("api/files", multipart, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                ? "File upload failed."
                : message);
        }

        var payload = await response.Content.ReadFromJsonAsync<UploadResponse>(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("File upload returned no body.");
        return payload.Path;
    }

    public async Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var fileName = Uri.EscapeDataString(Path.GetFileName(relativePath));
        using var response = await httpClient.DeleteAsync($"api/files/{fileName}", cancellationToken).ConfigureAwait(false);

        if (response.StatusCode is System.Net.HttpStatusCode.NotFound)
        {
            return;
        }

        response.EnsureSuccessStatusCode();
    }

    public string GetUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        var baseUrl = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
        var fileName = Uri.EscapeDataString(Path.GetFileName(relativePath));
        return $"{baseUrl}/api/files/{fileName}";
    }

    private sealed record UploadResponse(string Path);
}
