namespace PussyCats.App.RepositoryProxies;

public interface IFilesProxy
{
    Task<string> UploadAsync(Stream content, string originalFileName, CancellationToken cancellationToken = default);

    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);

    string GetUrl(string relativePath);
}
