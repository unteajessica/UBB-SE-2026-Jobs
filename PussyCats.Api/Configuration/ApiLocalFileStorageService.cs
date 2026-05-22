using PussyCats.Library.Services.FileStorage;

namespace PussyCats.Api.Configuration;

public sealed class ApiLocalFileStorageService : ILocalFileStorageService
{
    private readonly string uploadsPath = Path.Combine("uploads", "files");

    public ApiLocalFileStorageService()
    {
        Directory.CreateDirectory(uploadsPath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadsPath, fileName);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
        return fileName;
    }

    public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(uploadsPath, Path.GetFileName(relativePath));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(uploadsPath, Path.GetFileName(relativePath));
        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public string GetUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        var fileName = Path.GetFileName(relativePath);
        return $"/api/files/{Uri.EscapeDataString(fileName)}";
    }
}
