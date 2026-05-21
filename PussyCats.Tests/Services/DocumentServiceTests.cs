using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PussyCats.Library.Domain;
using PussyCats.Tests.Fakes;
using PussyCats.Library.Services.CvParsing;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.FileStorage;
using PussyCats.Library.Services.Users;

namespace PussyCats.Tests.Services;

public class DocumentServiceTests : IDisposable
{
    private readonly FakeDocumentRepository documentRepository;
    private readonly ILocalFileStorageService fileStorage;
    private readonly IUserService users;
    private readonly ICvParsingService cvParsing;
    private readonly DocumentService service;
    private readonly string temporaryPdfPath;

    public DocumentServiceTests()
    {
        documentRepository = new FakeDocumentRepository();
        fileStorage = Substitute.For<ILocalFileStorageService>();
        users = Substitute.For<IUserService>();
        cvParsing = Substitute.For<ICvParsingService>();
        service = new DocumentService(documentRepository, fileStorage, users, cvParsing);
        temporaryPdfPath = Path.Combine(Path.GetTempPath(), $"docsvc-{Guid.NewGuid():N}.pdf");
        File.WriteAllText(temporaryPdfPath, "%PDF-1.4 fake");
    }

    public void Dispose()
    {
        if (File.Exists(temporaryPdfPath))
        {
            File.Delete(temporaryPdfPath);
        }
    }


    [Fact]
    public async Task GetDocumentsByUserIdAsync_UserDoesNotExist_ReturnsEmptyList()
    {
        const int nonExistentUserId = 99;

        var documents = await service.GetDocumentsByUserIdAsync(nonExistentUserId);

        documents.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadDocumentAsync_UnsupportedExtensionProvided_RejectsUpload()
    {
        const int userId = 1;
        const string documentName = "Bad";
        var badPath = Path.Combine(Path.GetTempPath(), $"bad-{Guid.NewGuid():N}.exe");
        File.WriteAllText( badPath, "x");

        try
        {
            Func<Task> act = () => service.UploadDocumentAsync(
                new Document { User = new User { UserId = userId }, DocumentName = documentName }, badPath);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Invalid file type*");
        }
        finally
        {
            File.Delete(badPath);
        }
    }

    [Fact]
    public async Task UploadDocumentAsync_ValidDocumentProvided_PersistsMetadataWithStoragePath()
    {
        const int userId = 1;
        const string documentName = "CV";
        const string expectedStoragePath = "uploads/1.pdf";

        fileStorage.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(expectedStoragePath);
        var document = new Document { User = new User { UserId = userId }, DocumentName = documentName };

        var saved = await service.UploadDocumentAsync(document, temporaryPdfPath);

        saved.FilePath.Should().Be(expectedStoragePath);
    }

    [Fact]
    public async Task UploadDocumentAsync_ValidPngProvided_PersistsMetadata()
    {
        const int userId = 1;
        const string documentName = "screenshot.png";
        const string expectedStoragePath = "uploads/1.png";

        var temporaryPngPath = Path.Combine(Path.GetTempPath(), $"docsvc-{Guid.NewGuid():N}.png");
        File.WriteAllText(temporaryPngPath, "fake png content");
        fileStorage.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(expectedStoragePath);

        try
        {
            var saved = await service.UploadDocumentAsync(
                new Document { User = new User { UserId = userId }, DocumentName = documentName },
                temporaryPngPath);

            saved.FilePath.Should().Be(expectedStoragePath);
        }
        finally
        {
            File.Delete(temporaryPngPath);
        }
    }

    [Fact]
    public async Task UploadDocumentAsync_StorageServiceFails_SurfacesStorageException()
    {
        const int userId = 1;
        const string documentName = "CV";

        fileStorage.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("File upload failed."));

        Func<Task> act = () => service.UploadDocumentAsync(
            new Document { User = new User { UserId = userId }, DocumentName = documentName },
            temporaryPdfPath);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UploadDocumentFromStreamAsync_ValidDocumentProvided_SavesFileAndMetadata()
    {
        const int userId = 1;
        const string documentName = "Certificate";
        const string originalFileName = "certificate.pdf";
        const string expectedStoragePath = "uploads/certificate.pdf";

        users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(new User { UserId = userId });
        fileStorage.SaveFileAsync(Arg.Any<Stream>(), originalFileName, Arg.Any<CancellationToken>()).Returns(expectedStoragePath);

        await using var stream = new MemoryStream("pdf"u8.ToArray());
        var saved = await service.UploadDocumentFromStreamAsync(
            userId,
            documentName,
            originalFileName,
            "application/pdf",
            stream,
            isCv: false);

        saved.DocumentName.Should().Be(documentName);
        saved.FilePath.Should().Be(expectedStoragePath);
        saved.User.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task UploadDocumentFromStreamAsync_ValidCvProvided_ParsesCvAndUpdatesUser()
    {
        const int userId = 1;
        const string expectedStoragePath = "uploads/cv.json";
        var existingUser = new User { UserId = userId, FirstName = "Old", LastName = "Name" };
        var parsedUser = new User
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            Skills = new List<UserSkill>(),
            WorkExperiences = new List<WorkExperience>(),
            Projects = new List<Project>(),
            ExtraCurricularActivities = new List<ExtraCurricularActivity>(),
        };

        users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(existingUser);
        cvParsing.ParseCvFile(Arg.Any<string>(), ".json").Returns(parsedUser);
        fileStorage.SaveFileAsync(Arg.Any<Stream>(), "cv.json", Arg.Any<CancellationToken>()).Returns(expectedStoragePath);

        await using var stream = new MemoryStream("""{"firstName":"Ada"}"""u8.ToArray());
        var saved = await service.UploadDocumentFromStreamAsync(
            userId,
            "Ada CV",
            "cv.json",
            "application/json",
            stream,
            isCv: true);

        existingUser.FirstName.Should().Be("Ada");
        existingUser.LastName.Should().Be("Lovelace");
        saved.FilePath.Should().Be(expectedStoragePath);
        await users.Received(1).UpdateAsync(existingUser, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadDocumentFromStreamAsync_PartialCvProvided_PreservesExistingProfileFields()
    {
        const int userId = 1;
        const string expectedStoragePath = "uploads/cv.json";
        var existingSkills = new List<UserSkill>
        {
            new() { Skill = new Skill { Name = "C#" }, IsVerified = true, Score = 80 }
        };
        var existingUser = new User
        {
            UserId = userId,
            FirstName = "Existing",
            LastName = "User",
            Email = "existing@example.com",
            Skills = existingSkills,
        };
        var parsedUser = new User
        {
            FirstName = "Ada",
            Skills = new List<UserSkill>(),
            WorkExperiences = new List<WorkExperience>(),
            Projects = new List<Project>(),
            ExtraCurricularActivities = new List<ExtraCurricularActivity>(),
        };

        users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(existingUser);
        cvParsing.ParseCvFile(Arg.Any<string>(), ".json").Returns(parsedUser);
        fileStorage.SaveFileAsync(Arg.Any<Stream>(), "cv.json", Arg.Any<CancellationToken>()).Returns(expectedStoragePath);

        await using var stream = new MemoryStream("""{"firstName":"Ada"}"""u8.ToArray());
        await service.UploadDocumentFromStreamAsync(
            userId,
            "Ada CV",
            "cv.json",
            "application/json",
            stream,
            isCv: true);

        existingUser.FirstName.Should().Be("Ada");
        existingUser.LastName.Should().Be("User");
        existingUser.Email.Should().Be("existing@example.com");
        existingUser.Skills.Should().BeSameAs(existingSkills);
    }

    [Fact]
    public async Task UploadDocumentFromStreamAsync_UserMissing_ThrowsNotFound()
    {
        users.GetByIdAsync(404, Arg.Any<CancellationToken>()).Returns((User?)null);

        await using var stream = new MemoryStream("pdf"u8.ToArray());
        Func<Task> act = () => service.UploadDocumentFromStreamAsync(
            404,
            "Missing",
            "missing.pdf",
            "application/pdf",
            stream,
            isCv: false);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UploadDocumentFromStreamAsync_CvWithNonJsonExtension_RejectsUpload()
    {
        const int userId = 1;

        users.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(new User { UserId = userId });

        await using var stream = new MemoryStream("pdf"u8.ToArray());
        Func<Task> act = () => service.UploadDocumentFromStreamAsync(
            userId,
            "Bad CV",
            "cv.pdf",
            "application/pdf",
            stream,
            isCv: true);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only JSON*");
    }

    [Fact]
    public async Task DeleteDocumentAsync_DocumentExists_RemovesFileFromStorage()
    {
        const int documentId = 5;
        const int userId = 1;
        const string filePath = "uploads/x.pdf";

        documentRepository.Seed(new Document { DocumentId = documentId, User = new User { UserId = userId }, FilePath = filePath });

        await service.DeleteDocumentAsync(documentId);

        await fileStorage.Received(1).DeleteFileAsync(filePath, Arg.Any<CancellationToken>());
        (await documentRepository.GetByIdAsync(documentId)).Should().BeNull();
    }


    [Fact]
    public async Task DeleteDocumentAsync_DocumentIsMissing_ThrowsNotFoundException()
    {
        const int nonExistentDocumentId = 9999;
        Func<Task> act = () => service.DeleteDocumentAsync(nonExistentDocumentId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Document not found.");
    }

    [Fact]
    public async Task GetDocumentAbsolutePathAsync_DocumentExists_ReturnsStoragePath()
    {
        const int documentId = 5;
        const int userId = 1;
        const string relativePath = "uploads/x.pdf";
        const string absolutePath = @"C:\files\uploads\x.pdf";

        documentRepository.Seed(new Document { DocumentId = documentId, User = new User { UserId = userId }, FilePath = relativePath });
        fileStorage.GetFilePath(relativePath).Returns(absolutePath);

        var path = await service.GetDocumentAbsolutePathAsync(documentId);

        path.Should().Be(absolutePath);
    }

    [Fact]
    public async Task GetDocumentAbsolutePathAsync_DocumentIsMissing_ThrowsNotFoundException()
    {
        const int nonExistentDocumentId = 9999;
        Func<Task> act = () => service.GetDocumentAbsolutePathAsync(nonExistentDocumentId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Document not found.");
    }
}
