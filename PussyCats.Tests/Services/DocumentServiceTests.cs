using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.Services;

public class DocumentServiceTests : IDisposable
{
    private readonly FakeDocumentRepository documentRepository;
    private readonly ILocalFileStorageService fileStorage;
    private readonly DocumentService service;
    private readonly string tempPdfPath;

    public DocumentServiceTests()
    {
        documentRepository = new FakeDocumentRepository();
        fileStorage = Substitute.For<ILocalFileStorageService>();
        service = new DocumentService(documentRepository, fileStorage);
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

        var saved = await service.UploadDocumentAsync(document, tempPdfPath);

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
            tempPdfPath);

        await act.Should().ThrowAsync<InvalidOperationException>();
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