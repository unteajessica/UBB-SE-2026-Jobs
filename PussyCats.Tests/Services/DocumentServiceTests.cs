using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.Services;

public class DocumentServiceTests : IDisposable
{
    private readonly FakeDocumentRepository repo;
    private readonly ILocalFileStorageService fileStorage;
    private readonly DocumentService service;
    private readonly string tempPdfPath;

    public DocumentServiceTests()
    {
        repo = new FakeDocumentRepository();
        fileStorage = Substitute.For<ILocalFileStorageService>();
        service = new DocumentService(repo, fileStorage);
        tempPdfPath = Path.Combine(Path.GetTempPath(), $"docsvc-{Guid.NewGuid():N}.pdf");
        File.WriteAllText(tempPdfPath, "%PDF-1.4 fake");
    }

    public void Dispose()
    {
        if (File.Exists(tempPdfPath))
        {
            File.Delete(tempPdfPath);
        }
    }

    [Fact]
    public async Task GetDocumentsByUserIdAsync_UserHasDocuments_ReturnsUserDocuments()
    {
        repo.Seed(
            new Document { DocumentId = 1, User = new User { UserId = 1 }, DocumentName = "CV.pdf" },
            new Document { DocumentId = 2, User = new User { UserId = 2 }, DocumentName = "Other.pdf" });

        var returnedDocuments = await service.GetDocumentsByUserIdAsync(1);

        returnedDocuments.Should().HaveCount(1);
        returnedDocuments[0].DocumentName.Should().Be("CV.pdf");
    }

    [Fact]
    public async Task UploadDocumentAsync_UnsupportedExtensionProvided_RejectsUpload()
    {
        var badPath = Path.Combine(Path.GetTempPath(), $"bad-{Guid.NewGuid():N}.exe");
        File.WriteAllText(badPath, "x");

        try
        {
            Func<Task> act = () => service.UploadDocumentAsync(
                new Document { User = new User { UserId = 1 }, DocumentName = "Bad" },
                badPath);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Invalid file type*");
        }
        finally
        {
            File.Delete(badPath);
        }
    }

    [Fact]
    public async Task UploadDocumentAsync_ValidDocumentProvided_PersistsMetadataWithStoragePath()
    {
        fileStorage.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns("uploads/1.pdf");
        var document = new Document { User = new User { UserId = 1 }, DocumentName = "CV" };

        var saved = await service.UploadDocumentAsync(document, tempPdfPath);

        saved.FilePath.Should().Be("uploads/1.pdf");
        saved.UploadDate.Should().NotBe(default);
        saved.DocumentId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UploadDocumentAsync_StorageServiceFails_SurfacesStorageException()
    {
        fileStorage.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("File upload failed."));

        Func<Task> act = () => service.UploadDocumentAsync(
            new Document { User = new User { UserId = 1 }, DocumentName = "CV" },
            tempPdfPath);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteDocumentAsync_DocumentExists_RemovesFileAndMetadata()
    {
        repo.Seed(new Document { DocumentId = 5, User = new User { UserId = 1 }, FilePath = "uploads/x.pdf" });

        await service.DeleteDocumentAsync(5);

        await fileStorage.Received(1).DeleteFileAsync("uploads/x.pdf", Arg.Any<CancellationToken>());
        (await service.GetDocumentsByUserIdAsync(1)).Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteDocumentAsync_DocumentIsMissing_ThrowsNotFoundException()
    {
        Func<Task> act = () => service.DeleteDocumentAsync(404);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Document not found.");
    }

    [Fact]
    public async Task DeleteDocumentAsync_FilePathIsBlank_SkipsStorageCall()
    {
        repo.Seed(new Document { DocumentId = 5, User = new User { UserId = 1 }, FilePath = string.Empty });

        await service.DeleteDocumentAsync(5);

        await fileStorage.DidNotReceiveWithAnyArgs().DeleteFileAsync(default!, default);
    }

    [Fact]
    public async Task GetDocumentAbsolutePathAsync_DocumentExists_ReturnsStoragePath()
    {
        repo.Seed(new Document { DocumentId = 5, User = new User { UserId = 1 }, FilePath = "uploads/x.pdf" });
        fileStorage.GetFilePath("uploads/x.pdf").Returns(@"C:\files\uploads\x.pdf");

        var path = await service.GetDocumentAbsolutePathAsync(5);

        path.Should().Be(@"C:\files\uploads\x.pdf");
    }

    [Fact]
    public async Task GetDocumentAbsolutePathAsync_DocumentIsMissing_ThrowsNotFoundException()
    {
        Func<Task> act = () => service.GetDocumentAbsolutePathAsync(404);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Document not found.");
    }
}