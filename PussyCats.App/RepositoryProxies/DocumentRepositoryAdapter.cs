using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Documents;
using PussyCats.Library.Services.Documents;

namespace PussyCats.App.RepositoryProxies;

public class DocumentRepositoryAdapter : IDocumentRepository
{
    private readonly IDocumentService documents;

    public DocumentRepositoryAdapter(IDocumentService documents)
    {
        this.documents = documents;
    }

    public Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default)
        => documents.GetAllAsync(cancellationToken);

    public Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default)
        => documents.GetByIdAsync(documentId, cancellationToken);

    public Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => documents.GetDocumentsByUserIdAsync(userId, cancellationToken);

    public Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
        => documents.AddAsync(document, cancellationToken);

    public Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
        => documents.UpdateAsync(document, cancellationToken);

    public Task RemoveAsync(int documentId, CancellationToken cancellationToken = default)
        => documents.RemoveAsync(documentId, cancellationToken);
}
