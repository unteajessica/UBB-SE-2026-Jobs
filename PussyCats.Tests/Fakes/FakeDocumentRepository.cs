using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Documents;

namespace PussyCats.Tests.Fakes;

public class FakeDocumentRepository : IDocumentRepository
{
    private readonly Dictionary<int, Document> store = new();

    public void Seed(params Document[] documents)
    {
        foreach (var document in documents)
        {
            store[document.DocumentId] = document;
        }
    }

    public Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default)
    {
        store.TryGetValue(documentId, out var document);
        return Task.FromResult(document);
    }

    public Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Document> filtered = store.Values.Where(d => d.UserId == userId).ToList();
        return Task.FromResult(filtered);
    }

    public Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (document.DocumentId == 0)
        {
            document.DocumentId = NextId();
        }
        store[document.DocumentId] = document;
        return Task.FromResult(document);
    }

    public Task RemoveAsync(int documentId, CancellationToken cancellationToken = default)
    {
        store.Remove(documentId);
        return Task.CompletedTask;
    }

    private int NextId() => store.Count == 0 ? 1 : store.Keys.Max() + 1;
}
