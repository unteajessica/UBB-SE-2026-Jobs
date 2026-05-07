using System.Text.Json;

namespace PussyCats.App.Services;

public class ImageStorageService : IImageStorageService
{
    private string basePath = Path.Combine("uploads", "avatars");
    private const int BytesPerKilobyte = 1024;
    private const int BytesPerMegabyte = 1024 * BytesPerKilobyte;
    private const int MaxFileSizeInMb = 5;
    private const int MaxFileSize = MaxFileSizeInMb * BytesPerMegabyte;
    private readonly HashSet<string> allowedExtensions = new() { ".jpg", ".jpeg", ".png" };

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

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
    }
    public string SaveImage(Stream fileStream, string fileName)
    {
        var ext = GetImageExtension(fileName);
        CheckFileSize(fileStream);

        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", $"upload{ext}");

        using var http = new HttpClient();
        var response = http.PostAsync("https://localhost:7134/api/files", content).Result;

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
            throw new Exception("File size exceeds the maximum limit of " + MaxFileSize + "MB.");
        }
    }
}
