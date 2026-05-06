using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface IDocumentService
{
    Task<IReadOnlyList<Document>> GetDocumentsByUserIdAsync(int userId, CancellationToken ct = default);

    Task<Document> UploadDocumentAsync(Document document, string filePath, CancellationToken ct = default);

    Task DeleteDocumentAsync(int documentId, CancellationToken ct = default);

    Task<string> GetDocumentAbsolutePathAsync(int documentId, CancellationToken ct = default);
}
