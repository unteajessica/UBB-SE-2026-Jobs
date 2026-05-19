using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Tests.Helpers;
using PussyCats.Web.ServiceProxies;

namespace PussyCats.Tests.Web.ServiceProxies;

public class RecommendationServiceProxyTests
{
    private static RecommendationServiceProxy CreateProxy(StubHttpMessageHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

    [Fact]
    public async Task GetAllAsync_Returns200List_DeserializesIntoList()
    {
        var payload = new[]
        {
            new Recommendation { RecommendationId = 1, User = new UserBuilder().Build(), Job = new JobBuilder().Build() },
        };
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, JsonContent.Create(payload));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetAllAsync();

        result.Should().HaveCount(1);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/recommendations");
    }

    [Fact]
    public async Task GetByIdAsync_Returns200_DeserializesIntoEntity()
    {
        var entity = new Recommendation { RecommendationId = 5, User = new UserBuilder().Build(), Job = new JobBuilder().Build() };
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, JsonContent.Create(entity));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetByIdAsync(5);

        result!.RecommendationId.Should().Be(5);
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/api/recommendations/5");
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
    public async Task GetLatestForUserAndJobAsync_BuildsQueryString()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK,
            JsonContent.Create(new Recommendation { RecommendationId = 9, User = new UserBuilder().Build(), Job = new JobBuilder().Build() }));
        var proxy = CreateProxy(handler);

        await proxy.GetLatestForUserAndJobAsync(7, 8);

        handler.LastRequest!.RequestUri!.PathAndQuery.Should().Be("/api/recommendations?userId=7&jobId=8");
    }

    [Fact]
    public async Task AddAsync_PostsCorrectBody_AndReturnsCreatedEntity()
    {
        var saved = new Recommendation { RecommendationId = 12, User = new UserBuilder().Build(), Job = new JobBuilder().Build() };
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.Created, JsonContent.Create(saved));
        var proxy = CreateProxy(handler);

        var result = await proxy.AddAsync(1, 2, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        result.RecommendationId.Should().Be(12);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        var bodyJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        bodyDoc.RootElement.GetProperty("userId").GetInt32().Should().Be(1);
        bodyDoc.RootElement.GetProperty("jobId").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task UpdateTimestampAsync_PutsTimestampOnly()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.UpdateTimestampAsync(5, new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));

        handler.LastRequest!.Method.Should().Be(HttpMethod.Put);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/recommendations/5");
        var bodyJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        bodyDoc.RootElement.TryGetProperty("timestamp", out _).Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAsync_SendsDelete()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.RemoveAsync(5);

        handler.LastRequest!.Method.Should().Be(HttpMethod.Delete);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/recommendations/5");
    }
}

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode statusCode;
    private readonly HttpContent content;

    public HttpRequestMessage? LastRequest { get; private set; }

    private StubHttpMessageHandler(HttpStatusCode statusCode, HttpContent content)
    {
        this.statusCode = statusCode;
        this.content = content;
    }

    public static StubHttpMessageHandler Returning(HttpStatusCode statusCode, HttpContent content)
        => new(statusCode, content);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new HttpResponseMessage(statusCode) { Content = content });
    }
}
