using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Documents;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);

    Task RemoveAsync(int documentId, CancellationToken cancellationToken = default);
}
