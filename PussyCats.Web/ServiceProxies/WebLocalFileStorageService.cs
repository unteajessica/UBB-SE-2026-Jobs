using System.Text.Json;
using PussyCats.Library.Services.FileStorage;

namespace PussyCats.Web.ServiceProxies;

public class WebLocalFileStorageService : ILocalFileStorageService
{
    private readonly HttpClient http;

    public WebLocalFileStorageService(HttpClient http) => this.http = http;

    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", originalFileName);

        var response = await http.PostAsync("api/files", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<FileUploadResult>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken: cancellationToken);
        return result?.Path ?? throw new InvalidOperationException("API did not return a file path.");
    }

    public async Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/files/{Uri.EscapeDataString(relativePath)}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/files/{Uri.EscapeDataString(relativePath)}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public string GetFilePath(string relativePath)
    {
        var baseUrl = http.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
        return $"{baseUrl}/api/files/{relativePath}";
    }

    private record FileUploadResult(string Path);
}
