using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.Documents;

public interface IDocumentService
{
    Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Document>> GetDocumentsByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    // Metadata-only persistence. Used by the API; does not touch file storage.
    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);

    // Metadata-only removal. Used by the API; does not touch file storage.
    Task RemoveAsync(int documentId, CancellationToken cancellationToken = default);

    // Full upload flow with file storage. Used by the App.
    Task<Document> UploadDocumentAsync(Document document, string filePath, CancellationToken cancellationToken = default);

    // Removes both metadata and the underlying file. Used by the App.
    Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default);

    Task<string> GetDocumentAbsolutePathAsync(int documentId, CancellationToken cancellationToken = default);
}
