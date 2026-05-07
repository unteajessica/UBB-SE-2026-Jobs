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
    public async Task GetDocumentsByUserIdAsync_returns_user_documents()
    {
        repo.Seed(
            new Document { DocumentId = 1, UserId = 1, DocumentName = "CV.pdf" },
            new Document { DocumentId = 2, UserId = 2, DocumentName = "Other.pdf" });

        var result = await service.GetDocumentsByUserIdAsync(1);

        result.Should().HaveCount(1);
        result[0].DocumentName.Should().Be("CV.pdf");
    }

    [Fact]
    public async Task UploadDocumentAsync_rejects_unsupported_extension()
    {
        var badPath = Path.Combine(Path.GetTempPath(), $"bad-{Guid.NewGuid():N}.exe");
        File.WriteAllText(badPath, "x");

        try
        {
            Func<Task> act = () => service.UploadDocumentAsync(
                new Document { UserId = 1, DocumentName = "Bad" },
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
    public async Task UploadDocumentAsync_persists_metadata_with_storage_path()
    {
        fileStorage.SaveFile(Arg.Any<Stream>(), Arg.Any<string>()).Returns("uploads/1.pdf");
        var document = new Document { UserId = 1, DocumentName = "CV" };

        var saved = await service.UploadDocumentAsync(document, tempPdfPath);

        saved.FilePath.Should().Be("uploads/1.pdf");
        saved.UploadDate.Should().NotBe(default);
        saved.DocumentId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UploadDocumentAsync_surfaces_storage_NotImplementedException()
    {
        fileStorage.SaveFile(Arg.Any<Stream>(), Arg.Any<string>())
            .Throws(new NotImplementedException("Phase 5 routes uploads through /api/files"));

        Func<Task> act = () => service.UploadDocumentAsync(
            new Document { UserId = 1, DocumentName = "CV" },
            tempPdfPath);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task DeleteDocumentAsync_removes_file_and_metadata()
    {
        repo.Seed(new Document { DocumentId = 5, UserId = 1, FilePath = "uploads/x.pdf" });

        await service.DeleteDocumentAsync(5);

        fileStorage.Received(1).DeleteFile("uploads/x.pdf");
        (await service.GetDocumentsByUserIdAsync(1)).Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteDocumentAsync_throws_when_document_missing()
    {
        Func<Task> act = () => service.DeleteDocumentAsync(404);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Document not found.");
    }

    [Fact]
    public async Task DeleteDocumentAsync_skips_storage_call_for_blank_path()
    {
        repo.Seed(new Document { DocumentId = 5, UserId = 1, FilePath = string.Empty });

        await service.DeleteDocumentAsync(5);

        fileStorage.DidNotReceiveWithAnyArgs().DeleteFile(default!);
    }

    [Fact]
    public async Task GetDocumentAbsolutePathAsync_returns_storage_path()
    {
        repo.Seed(new Document { DocumentId = 5, UserId = 1, FilePath = "uploads/x.pdf" });
        fileStorage.GetFilePath("uploads/x.pdf").Returns(@"C:\files\uploads\x.pdf");

        var path = await service.GetDocumentAbsolutePathAsync(5);

        path.Should().Be(@"C:\files\uploads\x.pdf");
    }

    [Fact]
    public async Task GetDocumentAbsolutePathAsync_throws_when_document_missing()
    {
        Func<Task> act = () => service.GetDocumentAbsolutePathAsync(404);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Document not found.");
    }
}
