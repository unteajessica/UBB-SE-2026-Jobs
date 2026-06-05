using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.Users;
using PussyCats.Web.Controllers;
using PussyCats.Web.Models;
using PussyCats.Tests.Fakes;
using Xunit;

namespace PussyCats.Tests.Web.Controllers;

public class DocumentsControllerTests
{
    private readonly IDocumentService documents = Substitute.For<IDocumentService>();
    private readonly IUserService users = Substitute.For<IUserService>();
    private readonly DocumentsController controller;

    public DocumentsControllerTests()
    {
        controller = new DocumentsController(documents, users);
        users.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<User>());
    }

    [Fact]
    public async Task Index_ReturnsViewWithFilteredDocuments()
    {
        var user = new UserBuilder().WithId(1).Build();
        users.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { user });

        var docs = new List<Document>
        {
            new() { DocumentId = 10, DocumentName = "Resume", FilePath = "resume.pdf", User = user }
        };
        documents.GetDocumentsByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(docs);

        var result = await controller.Index(1, default);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(docs, viewResult.Model);
        Assert.Equal(1, (int)controller.ViewBag.SelectedUserId);
        Assert.Equal(1, (controller.ViewBag.Users as List<SelectListItem>).Count());
    }

    [Fact]
    public async Task Details_DocumentExists_ReturnsViewWithEntity()
    {
        var document = new Document { DocumentId = 15, DocumentName = "Certificate" };
        documents.GetByIdAsync(15, Arg.Any<CancellationToken>()).Returns(document);

        var result = await controller.Details(15, default);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(document, view.Model);
    }

    [Fact]
    public async Task Details_DocumentMissing_ReturnsNotFound()
    {
        documents.GetByIdAsync(404, Arg.Any<CancellationToken>()).Returns((Document?)null);

        var result = await controller.Details(404, default);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Get_PopulatesDropdownsAndReturnsEmptyModel()
    {
        users.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[] { new UserBuilder().WithId(2).Build() });

        var result = await controller.Create((int?)null, default);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<DocumentFormModel>(viewResult.Model);
        Assert.Equal(1, (controller.ViewBag.Users as List<SelectListItem>).Count());
    }

    [Fact]
    public async Task Create_Post_ValidModel_NoCv_CallsServiceAndRedirectsToIndex()
    {
        var user = new UserBuilder().WithId(1).Build();
        users.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);

        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("cert.pdf");
        file.Length.Returns(100);
        file.ContentType.Returns("application/pdf");
        file.OpenReadStream().Returns(new MemoryStream("mock"u8.ToArray()));

        var model = new DocumentFormModel
        {
            UserId = 1,
            DocumentName = "My Certificate",
            File = file,
        };

        var result = await controller.Create(model, default);

        await documents.Received(1).UploadDocumentFromStreamAsync(
            1,
            "My Certificate",
            "cert.pdf",
            "application/pdf",
            Arg.Any<Stream>(),
            false,
            Arg.Any<CancellationToken>());

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DocumentsController.Index), redirect.ActionName);
    }

    [Fact]
    public async Task Edit_Get_DocumentExists_PopulatesFormModel()
    {
        var user = new UserBuilder().WithId(1).Build();
        var document = new Document { DocumentId = 5, DocumentName = "Resume", FilePath = "resume.pdf", User = user };
        documents.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(document);

        var result = await controller.Edit(5, default);

        var model = Assert.IsType<DocumentFormModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(5, model.DocumentId);
        Assert.Equal(1, model.UserId);
        Assert.Equal("Resume", model.DocumentName);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_CallsServiceAndRedirects()
    {
        var user = new UserBuilder().WithId(1).Build();
        var document = new Document { DocumentId = 5, DocumentName = "Old Name", FilePath = "file.pdf", User = user };
        documents.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(document);

        var model = new DocumentFormModel { DocumentId = 5, UserId = 1, DocumentName = "New Name" };

        var result = await controller.Edit(5, model, default);

        await documents.Received(1).UpdateAsync(Arg.Is<Document>(document => 
            document.DocumentId == 5 && 
            document.DocumentName == "New Name"
        ), Arg.Any<CancellationToken>());

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DocumentsController.Index), redirect.ActionName);
    }

    [Fact]
    public async Task Delete_Get_DocumentExists_ReturnsView()
    {
        var document = new Document { DocumentId = 8, DocumentName = "OldDoc", User = new UserBuilder().WithId(1).Build() };
        documents.GetByIdAsync(8, Arg.Any<CancellationToken>()).Returns(document);

        var result = await controller.Delete(8, default);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(document, view.Model);
    }

    [Fact]
    public async Task DeleteConfirmed_CallsServiceAndRedirects()
    {
        var result = await controller.DeleteConfirmed(8, 1, default);

        await documents.Received(1).RemoveAsync(8, Arg.Any<CancellationToken>());
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(DocumentsController.Index), redirect.ActionName);
    }
}
