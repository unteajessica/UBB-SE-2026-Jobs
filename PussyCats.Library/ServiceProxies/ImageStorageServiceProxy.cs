using System.Net.Http.Json;
using System.Text.Json;
using PussyCats.Library.Services.ImageStorage;

namespace PussyCats.Library.ServiceProxies;

public class ImageStorageServiceProxy : IImageStorageService
{
    private const int MaxFileSizeInMb = 20;
    private const long MaxFileSize = MaxFileSizeInMb * 1024L * 1024L;

    private readonly HttpClient http;

    public ImageStorageServiceProxy(HttpClient http) => this.http = http;

    public async Task<string> SaveImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        CheckFileSize(fileStream);
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);

        var response = await http.PostAsync("api/files", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<FileUploadResult>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken: cancellationToken);
        return result?.Path ?? throw new InvalidOperationException("API did not return a file path.");
    }

    public async Task DeleteImageAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/files/{Uri.EscapeDataString(relativePath)}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return;
        response.EnsureSuccessStatusCode();
    }

    public void CheckFileSize(Stream fileStream)
    {
        if (fileStream.Length > MaxFileSize)
        {
            var mb = fileStream.Length / (1024.0 * 1024.0);
            throw new InvalidOperationException($"File size {mb:0.##} MB exceeds the {MaxFileSizeInMb} MB limit.");
        }
    }

    private record FileUploadResult(string Path);
}
