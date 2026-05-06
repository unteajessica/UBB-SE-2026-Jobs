using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Documents;

namespace PussyCats.App.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository documentRepository;
    private readonly ILocalFileStorageService fileStorage;

    public DocumentService(IDocumentRepository documentRepository, ILocalFileStorageService fileStorage)
    {
        this.documentRepository = documentRepository;
        this.fileStorage = fileStorage;
    }

    public async Task<IReadOnlyList<Document>> GetDocumentsByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await documentRepository.GetByUserIdAsync(userId, ct).ConfigureAwait(false);
    }

    public async Task<Document> UploadDocumentAsync(Document document, string filePath, CancellationToken ct = default)
    {
        string extension = Path.GetExtension(filePath);

        if (!ValidateFileType(extension))
        {
            throw new InvalidOperationException(
                "Invalid file type. Only PDF, JPG, and PNG files are accepted.");
        }

        using var stream = File.OpenRead(filePath);
        string relativePath = fileStorage.SaveFile(stream, Path.GetFileName(filePath));

        document.FilePath = relativePath;
        document.UploadDate = DateTime.Now;

        return await documentRepository.AddAsync(document, ct).ConfigureAwait(false);
    }

    public async Task DeleteDocumentAsync(int documentId, CancellationToken ct = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, ct).ConfigureAwait(false);

        if (document is null)
        {
            throw new InvalidOperationException("Document not found.");
        }

        if (!string.IsNullOrEmpty(document.FilePath))
        {
            fileStorage.DeleteFile(document.FilePath);
        }

        await documentRepository.RemoveAsync(documentId, ct).ConfigureAwait(false);
    }

    public async Task<string> GetDocumentAbsolutePathAsync(int documentId, CancellationToken ct = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, ct).ConfigureAwait(false);

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
