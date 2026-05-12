using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.ViewModels;

public class DocumentViewModelTests
{
    private readonly IDocumentService documentService = Substitute.For<IDocumentService>();
    private readonly IUserService userService = Substitute.For<IUserService>();
    private readonly SessionContext session = new() { UserId = 22 };

    [Fact]
    public async Task LoadDocumentsAsync_DocumentsExist_PopulatesDocumentList()
    {
        documentService.GetDocumentsByUserIdAsync(22, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Document>>([new() { DocumentId = 1, DocumentName = "CV", User = new User { UserId = 22 } }]));
        var viewModel = new DocumentListViewModel(documentService, session);

        await viewModel.LoadDocumentsAsync();

        viewModel.GetDocuments().Should().ContainSingle(document => document.DocumentName == "CV");
    }

    [Fact]
    public async Task DeleteDocumentAsync_ValidId_InvokesDeleteAndRefreshesList()
    {
        documentService.GetDocumentsByUserIdAsync(22, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Document>>([]));
        var viewModel = new DocumentListViewModel(documentService, session);

        await viewModel.DeleteDocumentAsync(1);

        await documentService.Received(1).DeleteDocumentAsync(1, Arg.Any<CancellationToken>());
        await documentService.Received(1).GetDocumentsByUserIdAsync(22, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadDocumentAsync_InputIsValid_CallsServiceUpload()
    {
        userService.GetByIdAsync(22, Arg.Any<CancellationToken>()).Returns(new User { UserId = 22 });
        var viewModel = new UploadDocumentViewModel(documentService, userService, session)
        {
            DocumentName = "CV",
            SelectedFilePath = "cv.pdf",
        };

        await viewModel.UploadDocumentAsync();

        await documentService.Received(1).UploadDocumentAsync(
            Arg.Is<Document>(document => document.User.UserId == 22 && document.DocumentName == "CV"),
            "cv.pdf",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ValidateDocumentInput_EmptyFields_ReportsValidationErrors()
    {
        var viewModel = new UploadDocumentViewModel(documentService, userService, session);

        viewModel.ValidateDocumentInput().Should().BeFalse();
        viewModel.GetErrorMessage().Should().Contain("Document name");

        viewModel.DocumentName = "CV";
        viewModel.ValidateDocumentInput().Should().BeFalse();
        viewModel.GetErrorMessage().Should().Contain("Select a file");
    }
}