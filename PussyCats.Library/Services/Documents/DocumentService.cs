using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Documents;
using PussyCats.Library.Services.FileStorage;

namespace PussyCats.Library.Services.Documents;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository documentRepository;
    private readonly ILocalFileStorageService fileStorage;

    public DocumentService(IDocumentRepository documentRepository, ILocalFileStorageService fileStorage)
    {
        this.documentRepository = documentRepository;
        this.fileStorage = fileStorage;
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await documentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Document?> GetByIdAsync(int documentId, CancellationToken cancellationToken = default)
    {
        return await documentRepository.GetByIdAsync(documentId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Document>> GetDocumentsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await documentRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        return await documentRepository.AddAsync(document, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        await documentRepository.UpdateAsync(document, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int documentId, CancellationToken cancellationToken = default)
    {
        await documentRepository.RemoveAsync(documentId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Document> UploadDocumentAsync(Document document, string filePath, CancellationToken cancellationToken = default)
    {
        string extension = Path.GetExtension(filePath);

        if (!ValidateFileType(extension))
        {
            throw new InvalidOperationException(
                "Invalid file type. Only PDF, JPG, and PNG files are accepted.");
        }

        using var stream = File.OpenRead(filePath);
        string relativePath = await fileStorage.SaveFileAsync(stream, Path.GetFileName(filePath), cancellationToken).ConfigureAwait(false);

        document.FilePath = relativePath;
        document.UploadDate = DateTime.Now;

        return await documentRepository.AddAsync(document, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken).ConfigureAwait(false);

        if (document is null)
        {
            throw new InvalidOperationException("Document not found.");
        }

        if (!string.IsNullOrEmpty(document.FilePath))
        {
            await fileStorage.DeleteFileAsync(document.FilePath, cancellationToken).ConfigureAwait(false);
        }

        await documentRepository.RemoveAsync(documentId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> GetDocumentAbsolutePathAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken).ConfigureAwait(false);

        if (document is null)
        {
            throw new InvalidOperationException("Document not found.");
        }

        return fileStorage.GetFilePath(document.FilePath);
    }

    private static bool ValidateFileType(string extension)
    {
        string normalisedExtension = extension.TrimStart('.');
        return Enum.TryParse<AllowedFileType>(normalisedExtension, ignoreCase: true, out _);
    }
}
