using PussyCats.Library.Services.FileStorage;

namespace PussyCats.Api.Configuration;

// API never serves files through ILocalFileStorageService. The interface lives in
// Library because DocumentService depends on it; the App owns real upload/download
// via /api/files (FilesController, currently 501). DocumentsController in this
// project uses only the metadata methods (GetByIdAsync, AddAsync, RemoveAsync,
// GetDocumentsByUserIdAsync), so this stub is never hit in normal flow. It
// throws loudly if something accidentally routes a file call through the API.
public sealed class StubLocalFileStorageService : ILocalFileStorageService
{
    public Task<string> SaveFileAsync(Stream fileStream, string originalFileName, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("File uploads route through FilesController (/api/files), not the API's DocumentService.");

    public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("File deletes route through FilesController (/api/files), not the API's DocumentService.");

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("File reads route through FilesController (/api/files), not the API's DocumentService.");

    public string GetFilePath(string relativePath)
        => throw new NotSupportedException("Path resolution routes through FilesController (/api/files), not the API's DocumentService.");
}
