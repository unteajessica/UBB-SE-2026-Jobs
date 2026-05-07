using System.Text.Json;
using PussyCats.App.Configuration;

namespace PussyCats.App.Services;

public class ImageStorageService : IImageStorageService
{
    private string basePath = Path.Combine("uploads", "avatars");
    private const int BytesPerKilobyte = 1024;
    private const int BytesPerMegabyte = 1024 * BytesPerKilobyte;
    private const int MaxFileSizeInMb = 20;
    private const int MaxFileSize = MaxFileSizeInMb * BytesPerMegabyte;
    private readonly HashSet<string> allowedExtensions = new() { ".jpg", ".jpeg", ".png" };
    private readonly bool useApiStorage = true;
    private readonly string apiBaseUrl = ApiConfigurationLoader.Load().BaseUrl.TrimEnd('/');

    public ImageStorageService()
    {
        string fullDirectoryPath = Path.Combine(AppContext.BaseDirectory, basePath);

        if (!Directory.Exists(fullDirectoryPath))
        {
            Directory.CreateDirectory(fullDirectoryPath);
        }
    }

    public ImageStorageService(string basePath)
    {
        this.basePath = basePath;
        useApiStorage = false;

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
    }
    public string SaveImage(Stream fileStream, string fileName)
    {
        var ext = GetImageExtension(fileName);
        CheckFileSize(fileStream);

        if (!useApiStorage)
        {
            var storedFileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(basePath, storedFileName);
            using var output = File.Create(fullPath);
            fileStream.CopyTo(output);
            return Path.Combine(basePath, storedFileName);
        }

        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", $"upload{ext}");

        using var http = new HttpClient();
        var response = http.PostAsync($"{apiBaseUrl}/api/files", content).Result;

        if (!response.IsSuccessStatusCode)
            throw new Exception("File upload failed.");

        var json = response.Content.ReadAsStringAsync().Result;
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("path").GetString()!;
    }

    /* public string SaveImage(Stream fileStream, string fileName)
     {
         // Phase 5 routes uploads through /api/files; silent disk writes during
         // demo would mask the bug.
         throw new NotImplementedException(
             "Phase 5 routes file uploads through /api/files per MergePlan §4.");
     }*/

    public void DeleteImage(string relativePath)
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

        string fullPath = Path.Combine(AppContext.BaseDirectory, basePath, Path.GetFileName(relativePath));
        if (!Path.IsPathRooted(fullPath))
        {
            fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private string GetImageExtension(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"Unsupported file type. Allowed formats are: {string.Join(", ", allowedExtensions.Order())}");
        }

        return extension;
    }

    public void CheckFileSize(Stream fileStream)
    {
        if (fileStream.Length > MaxFileSize)
        {
            var fileSizeInMb = fileStream.Length / (double)BytesPerMegabyte;
            throw new InvalidOperationException(
                $"File size exceeds the maximum limit of {MaxFileSizeInMb} MB. Selected file is {fileSizeInMb:0.##} MB.");
        }
    }
}
