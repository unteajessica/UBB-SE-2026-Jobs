using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.Documents;

public interface ILocalDocumentFileService
{
    Task<Document> UploadDocumentAsync(Document document, string filePath, CancellationToken cancellationToken = default);

    Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default);

    Task<string> GetDocumentUrlAsync(int documentId, CancellationToken cancellationToken = default);
}
