using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Documents;

namespace PussyCats.Tests.Fakes;

public class FakeDocumentRepository : IDocumentRepository
{
    private readonly Dictionary<int, Document> documentsById = new();

    public void Seed(params Document[] documents)
    {
        foreach (var document in documents)
        {
            documentsById[document.DocumentId] = document;
        }
    }

    public Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default)
    {
        documentsById.TryGetValue(documentId, out var document);
        return Task.FromResult(document);
    }

    public Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Document> filtered = documentsById.Values.Where(document => document.User.UserId == userId).ToList();

        return Task.FromResult(filtered);
    }

    public Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (document.DocumentId == 0)
        {
            document.DocumentId = NextId();
        }
        documentsById[document.DocumentId] = document;
        return Task.FromResult(document);
    }

    public Task RemoveAsync(int documentId, CancellationToken cancellationToken = default)
    {
        documentsById.Remove(documentId);
        return Task.CompletedTask;
    }

    private int NextId() => documentsById.Count == 0 ? 1 : documentsById.Keys.Max() + 1;
}
