using System.Text.Json;
using PussyCats.App.Configuration;

namespace PussyCats.App.Services;

public class LocalFileStorageService : ILocalFileStorageService
{
    private string basePath = Path.Combine("uploads", "documents");
    private readonly bool useApiStorage = true;
    private readonly string apiBaseUrl = ApiConfigurationLoader.Load().BaseUrl.TrimEnd('/');

    public LocalFileStorageService()
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, basePath);
        Directory.CreateDirectory(fullPath);
    }

    public LocalFileStorageService(string basePath)
    {
        this.basePath = basePath;
        useApiStorage = false;
        Directory.CreateDirectory(basePath);
    }

    public string SaveFile(Stream fileStream, string originalFileName)
    {
        if (useApiStorage)
        {
            return UploadToApi(fileStream, originalFileName);
        }

        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(basePath, storedFileName);
        using var output = File.Create(fullPath);
        fileStream.CopyTo(output);
        return Path.Combine(basePath, storedFileName);
    }

    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        if (useApiStorage)
        {
            using var http = new HttpClient();
            _ = http.DeleteAsync($"{apiBaseUrl}/api/files/{Uri.EscapeDataString(Path.GetFileName(relativePath))}").Result;
            return;
        }

        var resolvedFileFullPath = Path.Combine(AppContext.BaseDirectory, basePath, Path.GetFileName(relativePath));
        if (!Path.IsPathRooted(resolvedFileFullPath))
        {
            resolvedFileFullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        }

        if (File.Exists(resolvedFileFullPath))
        {
            File.Delete(resolvedFileFullPath);
        }
    }

    public string GetFilePath(string relativePath)
    {
        if (relativePath == null)
        {
            throw new ArgumentNullException(nameof(relativePath));
        }

        if (useApiStorage)
        {
            return $"{apiBaseUrl}/api/files/{Uri.EscapeDataString(Path.GetFileName(relativePath))}";
        }

        var returnedPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (!Path.Exists(returnedPath))
        {
            throw new FileNotFoundException($"File not found at path: {relativePath}");
        }

        return returnedPath;
    }

    private string UploadToApi(Stream fileStream, string originalFileName)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", Path.GetFileName(originalFileName));

        using var http = new HttpClient();
        using var response = http.PostAsync($"{apiBaseUrl}/api/files", content).Result;
        if (!response.IsSuccessStatusCode)
        {
            var message = response.Content.ReadAsStringAsync().Result;
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                ? "File upload failed."
                : message);
        }

        var json = response.Content.ReadAsStringAsync().Result;
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("path").GetString()!;
    }
}
