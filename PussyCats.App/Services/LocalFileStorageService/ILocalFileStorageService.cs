namespace PussyCats_App.Services.LocalFileStorageService;

public interface ILocalFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default);

    Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);

    string GetFilePath(string relativePath);
}
