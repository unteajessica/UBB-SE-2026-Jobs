using PussyCats.Library.Domain;

namespace PussyCats_App.Services.DocumentService;

public interface IDocumentService
{
    Task<IReadOnlyList<Document>> GetDocumentsByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Document> UploadDocumentAsync(Document document, string filePath, CancellationToken cancellationToken = default);

    Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default);

    Task<string> GetDocumentAbsolutePathAsync(int documentId, CancellationToken cancellationToken = default);
}
