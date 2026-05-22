using PussyCats.App.RepositoryProxies;
using PussyCats.Library.Services.FileStorage;

namespace PussyCats_App.Services.LocalFileStorageService;

public class LocalFileStorageService : ILocalFileStorageService
{
    private readonly IFilesProxy filesProxy;

    public LocalFileStorageService(IFilesProxy filesProxy)
    {
        this.filesProxy = filesProxy;
    }

    public Task<string> SaveFileAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
    {
        return filesProxy.UploadAsync(fileStream, originalFileName, cancellationToken);
    }

    public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        return filesProxy.DeleteAsync(relativePath, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        return filesProxy.DownloadAsync(relativePath, cancellationToken);
    }

    public string GetUrl(string relativePath)
    {
        if (relativePath == null)
        {
            throw new ArgumentNullException(nameof(relativePath));
        }

        return filesProxy.GetUrl(relativePath);
    }
}
