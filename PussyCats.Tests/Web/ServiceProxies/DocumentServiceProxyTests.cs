using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using PussyCats.Library.Domain;
using PussyCats.Tests.Helpers;
using PussyCats.Library.ServiceProxies;
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

        Assert.Equal(5, result!.DocumentId);
        Assert.Equal("Resume", result.DocumentName);
        Assert.Equal("/api/documents/5", handler.LastRequest!.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetByIdAsync_Returns404_ReturnsNull()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NotFound, new StringContent(""));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetByIdAsync(404);

        Assert.Null(result);
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

        Assert.Equal(1, result.Count());
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Equal("/api/documents?userId=1", handler.LastRequest.RequestUri!.PathAndQuery);
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

        Assert.Equal(99, result.DocumentId);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        var bodyJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        Assert.Equal(10, bodyDoc.RootElement.GetProperty("userId").GetInt32());
        Assert.Equal("New Doc", bodyDoc.RootElement.GetProperty("documentName").GetString());
        Assert.Equal("new.pdf", bodyDoc.RootElement.GetProperty("filePath").GetString());
    }

    [Fact]
    public async Task RemoveAsync_SendsDelete()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.RemoveAsync(5);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest!.Method);
        Assert.Equal("/api/documents/5", handler.LastRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task UpdateAsync_SendsPut()
    {
        var user = new UserBuilder().WithId(10).Build();
        var document = new Document { DocumentId = 5, DocumentName = "Updated Name", FilePath = "resume.pdf", User = user };
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.UpdateAsync(document);

        Assert.Equal(HttpMethod.Put, handler.LastRequest!.Method);
        Assert.Equal("/api/documents/5", handler.LastRequest.RequestUri!.AbsolutePath);
        var bodyJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        Assert.Equal("Updated Name", bodyDoc.RootElement.GetProperty("documentName").GetString());
        Assert.Equal("resume.pdf", bodyDoc.RootElement.GetProperty("filePath").GetString());
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

        Assert.Equal(99, result.DocumentId);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("/api/documents/upload", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.IsType<MultipartFormDataContent>(handler.LastRequest.Content);
    }

}
