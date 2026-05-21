using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Documents;
using PussyCats.Library.Services.CvParsing;
using PussyCats.Library.Services.FileStorage;
using PussyCats.Library.Services.Users;

namespace PussyCats.Library.Services.Documents;

public class DocumentService : IDocumentService, ILocalDocumentFileService
{
    private const string JsonExtension = ".json";
    private const string InvalidDocumentTypeMessage = "Invalid file type. Only PDF, JPG, and PNG files are accepted.";
    private const string InvalidCvTypeMessage = "Invalid CV file type. Only JSON files are accepted for CV parsing.";
    private const string DocumentNotFoundMessage = "Document not found.";
    private const string UserNotFoundMessage = "Selected user does not exist.";

    private readonly IDocumentRepository documentRepository;
    private readonly ILocalFileStorageService fileStorage;
    private readonly IUserService users;
    private readonly ICvParsingService cvParsing;

    public DocumentService(
        IDocumentRepository documentRepository,
        ILocalFileStorageService fileStorage,
        IUserService users,
        ICvParsingService cvParsing)
    {
        this.documentRepository = documentRepository;
        this.fileStorage = fileStorage;
        this.users = users;
        this.cvParsing = cvParsing;
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

    public async Task<Document> UploadDocumentFromStreamAsync(
        int userId,
        string documentName,
        string originalFileName,
        string contentType,
        Stream fileStream,
        bool isCv,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            throw new KeyNotFoundException(UserNotFoundMessage);
        }

        var extension = Path.GetExtension(originalFileName);
        ValidateUploadFileType(extension, isCv);

        Stream streamToSave = fileStream;
        MemoryStream? cvStream = null;

        if (isCv)
        {
            using var reader = new StreamReader(fileStream, leaveOpen: true);
            var jsonContent = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            User parsedUser;
            try
            {
                parsedUser = cvParsing.ParseCvFile(jsonContent, JsonExtension);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(exception.Message, exception);
            }

            ApplyParsedCvData(user, parsedUser);
            await users.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
            cvStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent));
            streamToSave = cvStream;
        }

        string relativePath;
        try
        {
            relativePath = await fileStorage.SaveFileAsync(streamToSave, originalFileName, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            cvStream?.Dispose();
        }

        var document = new Document
        {
            User = user,
            DocumentName = documentName,
            FilePath = relativePath,
            UploadDate = DateTime.UtcNow,
        };

        return await documentRepository.AddAsync(document, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Document> UploadDocumentAsync(Document document, string filePath, CancellationToken cancellationToken = default)
    {
        string extension = Path.GetExtension(filePath);

        if (!ValidateFileType(extension))
        {
            throw new InvalidOperationException(
                InvalidDocumentTypeMessage);
        }

        using var stream = File.OpenRead(filePath);
        string relativePath = await fileStorage.SaveFileAsync(stream, Path.GetFileName(filePath), cancellationToken).ConfigureAwait(false);

        document.FilePath = relativePath;
        document.UploadDate = DateTime.UtcNow;

        return await documentRepository.AddAsync(document, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteDocumentAsync(int documentId, CancellationToken cancellationToken = default)
    {
        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken).ConfigureAwait(false);

        if (document is null)
        {
            throw new InvalidOperationException(DocumentNotFoundMessage);
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
            throw new InvalidOperationException(DocumentNotFoundMessage);
        }

        return fileStorage.GetFilePath(document.FilePath);
    }

    private static bool ValidateFileType(string extension)
    {
        string normalisedExtension = extension.TrimStart('.');
        return Enum.TryParse<AllowedFileType>(normalisedExtension, ignoreCase: true, out _);
    }

    private static void ValidateUploadFileType(string extension, bool isCv)
    {
        if (isCv)
        {
            if (!string.Equals(extension, JsonExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(InvalidCvTypeMessage);
            }

            return;
        }

        if (!ValidateFileType(extension))
        {
            throw new InvalidOperationException(InvalidDocumentTypeMessage);
        }
    }

    private static void ApplyParsedCvData(User target, User parsed)
    {
        ApplyIfPresent(parsed.FirstName, value => target.FirstName = value);
        ApplyIfPresent(parsed.LastName, value => target.LastName = value);
        ApplyIfPresent(parsed.Gender, value => target.Gender = value);
        ApplyIfPresent(parsed.Email, value => target.Email = value);
        ApplyIfPresent(parsed.Phone, value => target.Phone = value);
        ApplyIfPresent(parsed.Country, value => target.Country = value);
        ApplyIfPresent(parsed.City, value => target.City = value);
        ApplyIfPresent(parsed.University, value => target.University = value);
        ApplyIfPresent(parsed.GitHub, value => target.GitHub = value);
        ApplyIfPresent(parsed.LinkedIn, value => target.LinkedIn = value);
        ApplyIfPresent(parsed.Address, value => target.Address = value);
        ApplyIfPresent(parsed.Motivation, value => target.Motivation = value);

        if (parsed.Age > 0)
        {
            target.Age = parsed.Age;
        }

        if (parsed.ExpectedGraduationYear > 0)
        {
            target.ExpectedGraduationYear = parsed.ExpectedGraduationYear;
        }

        if (parsed.HasDisabilities)
        {
            target.HasDisabilities = true;
        }

        if (parsed.Skills.Count > 0)
        {
            target.Skills = parsed.Skills;
        }

        if (parsed.WorkExperiences.Count > 0)
        {
            target.WorkExperiences = parsed.WorkExperiences;
        }

        if (parsed.Projects.Count > 0)
        {
            target.Projects = parsed.Projects;
        }

        if (parsed.ExtraCurricularActivities.Count > 0)
        {
            target.ExtraCurricularActivities = parsed.ExtraCurricularActivities;
        }
    }

    private static void ApplyIfPresent(string value, Action<string> apply)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            apply(value);
        }
    }
}
