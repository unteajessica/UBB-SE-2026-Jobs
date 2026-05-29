using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Documents;

public class DocumentRepository : IDocumentRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public DocumentRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.Documents
            .AsNoTracking()
            .Include(document => document.User)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Tracked — typical caller (FilesController.Delete) mutates immediately. No User include
    /// because the path is already enough to serve the file.
    /// </summary>
    public async Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Documents
            .Include(document => document.User)
            .FirstOrDefaultAsync(document => document.DocumentId == documentId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp DocumentRepository.GetDocumentsByUserId — straight predicate port.
    /// Read-only listing.
    /// </summary>
    public async Task<IReadOnlyList<Document>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Documents
            .AsNoTracking()
            .Include(document => document.User)
            .Where(document => document.User.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        if (document.UploadDate == default)
        {
            document.UploadDate = DateTime.UtcNow;
        }
        databaseContext.Documents.Add(document);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return document;
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        var existing = await databaseContext.Documents.FindAsync(new object?[] { document.DocumentId }, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            existing.DocumentName = document.DocumentName;
            existing.FilePath = document.FilePath;
            await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task RemoveAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await databaseContext.Documents.FindAsync(new object?[] { documentId }, cancellationToken).ConfigureAwait(false);
        if (document is null)
        {
            return;
        }
        databaseContext.Documents.Remove(document);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

