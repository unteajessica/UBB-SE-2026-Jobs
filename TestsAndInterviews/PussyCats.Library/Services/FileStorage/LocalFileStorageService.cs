using PussyCats.Library.Services.FileStorage;

namespace PussyCats.Library.Services.FileStorage;

public sealed class LocalFileStorageService : ILocalFileStorageService
{
    private readonly string uploadsPath;

    public LocalFileStorageService(string uploadsPath)
    {
        this.uploadsPath = uploadsPath;

        Directory.CreateDirectory(this.uploadsPath);
    }

    public async Task<string> SaveFileAsync(
        Stream fileStream,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadsPath, fileName);

        await using var output = File.Create(fullPath);

        await fileStream.CopyToAsync(output, cancellationToken);

        return fileName;
    }

    public Task DeleteFileAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(
            uploadsPath,
            Path.GetFileName(relativePath));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(
            uploadsPath,
            Path.GetFileName(relativePath));

        Stream stream = File.OpenRead(fullPath);

        return Task.FromResult(stream);
    }

    public string GetUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        var fileName = Uri.EscapeDataString(
            Path.GetFileName(relativePath));

        return $"/api/files/{fileName}";
    }
}
