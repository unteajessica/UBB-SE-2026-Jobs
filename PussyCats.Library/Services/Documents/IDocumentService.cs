using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.Documents;

public interface IDocumentService
{
    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Document>> GetDocumentsByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);

    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);

    Task RemoveAsync(int documentId, CancellationToken cancellationToken = default);

    Task<Document> UploadDocumentAsync(Document document, string filePath, CancellationToken cancellationToken = default);

    Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default);

    Task<string> GetDocumentAbsolutePathAsync(int documentId, CancellationToken cancellationToken = default);
}
