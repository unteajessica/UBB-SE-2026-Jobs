using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Tests.Helpers;
using PussyCats.Web.ServiceProxies;
using Xunit;

namespace PussyCats.Tests.Web.ServiceProxies;

public class DocumentServiceProxyTests
{
    private static DocumentServiceProxy CreateProxy(StubHttpMessageHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

    [Fact]
    public async Task GetByIdAsync_Returns200_DeserializesIntoEntity()
    {
        var entity = new Document { DocumentId = 5, DocumentName = "Resume", FilePath = "resume.pdf" };
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, JsonContent.Create(entity));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetByIdAsync(5);

        result!.DocumentId.Should().Be(5);
        result.DocumentName.Should().Be("Resume");
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/api/documents/5");
    }

    [Fact]
    public async Task GetByIdAsync_Returns404_ReturnsNull()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NotFound, new StringContent(""));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetByIdAsync(404);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDocumentsByUserIdAsync_Returns200List_DeserializesIntoList()
    {
        var payload = new[]
        {
            new Document { DocumentId = 1, DocumentName = "Doc 1", FilePath = "1.pdf" }
        };
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, JsonContent.Create(payload));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetDocumentsByUserIdAsync(1);

        result.Should().HaveCount(1);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be("/api/documents?userId=1");
    }

    [Fact]
    public async Task AddAsync_PostsCorrectBody_AndReturnsCreatedEntity()
    {
        var user = new UserBuilder().WithId(10).Build();
        var document = new Document { DocumentName = "New Doc", FilePath = "new.pdf", User = user };
        var saved = new Document { DocumentId = 99, DocumentName = "New Doc", FilePath = "new.pdf", User = user };

        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.Created, JsonContent.Create(saved));
        var proxy = CreateProxy(handler);

        var result = await proxy.AddAsync(document);

        result.DocumentId.Should().Be(99);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        var bodyJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        bodyDoc.RootElement.GetProperty("userId").GetInt32().Should().Be(10);
        bodyDoc.RootElement.GetProperty("documentName").GetString().Should().Be("New Doc");
        bodyDoc.RootElement.GetProperty("filePath").GetString().Should().Be("new.pdf");
    }

    [Fact]
    public async Task RemoveAsync_SendsDelete()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.RemoveAsync(5);

        handler.LastRequest!.Method.Should().Be(HttpMethod.Delete);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/documents/5");
    }

    [Fact]
    public async Task UpdateAsync_SendsPut()
    {
        var user = new UserBuilder().WithId(10).Build();
        var document = new Document { DocumentId = 5, DocumentName = "Updated Name", FilePath = "resume.pdf", User = user };
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.UpdateAsync(document);

        handler.LastRequest!.Method.Should().Be(HttpMethod.Put);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/documents/5");
        var bodyJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        bodyDoc.RootElement.GetProperty("documentName").GetString().Should().Be("Updated Name");
        bodyDoc.RootElement.GetProperty("filePath").GetString().Should().Be("resume.pdf");
    }

    [Fact]
    public async Task UploadDocumentFromStreamAsync_SendsMultipartUpload()
    {
        var user = new UserBuilder().WithId(10).Build();
        var saved = new Document { DocumentId = 99, DocumentName = "CV", FilePath = "cv.json", User = user };
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.Created, JsonContent.Create(saved));
        var proxy = CreateProxy(handler);

        await using var stream = new MemoryStream("""{"firstName":"Ada"}"""u8.ToArray());
        var result = await proxy.UploadDocumentFromStreamAsync(
            10,
            "CV",
            "cv.json",
            "application/json",
            stream,
            isCv: true);

        result.DocumentId.Should().Be(99);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/documents/upload");
        handler.LastRequest.Content.Should().BeOfType<MultipartFormDataContent>();
    }

}
