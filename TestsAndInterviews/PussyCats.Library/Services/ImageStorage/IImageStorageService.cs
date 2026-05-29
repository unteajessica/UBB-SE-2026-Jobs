namespace PussyCats.Library.Services.ImageStorage;

public interface IImageStorageService
{
    Task<string> SaveImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    Task DeleteImageAsync(string relativePath, CancellationToken cancellationToken = default);

    void CheckFileSize(Stream fileStream);
}
