using PussyCats.App.RepositoryProxies;

namespace PussyCats.App.Services;

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

    public string GetFilePath(string relativePath)
    {
        if (relativePath == null)
        {
            throw new ArgumentNullException(nameof(relativePath));
        }

        return filesProxy.GetUrl(relativePath);
    }
}
