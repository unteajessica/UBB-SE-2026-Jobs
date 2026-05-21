using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Helpers;
using PussyCats.Web.ServiceProxies;

namespace PussyCats.Tests.Web.ServiceProxies;

public class MatchServiceProxyTests
{
    private static MatchServiceProxy CreateProxy(StubHttpMessageHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

    [Fact]
    public async Task GetByCompanyIdAsync_BuildsCompanyQueryString()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, JsonContent.Create(new[] { CreateMatch() }));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetByCompanyIdAsync(42);

        result.Should().HaveCount(1);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be("/api/matches?companyId=42");
    }

    [Fact]
    public async Task SubmitDecisionAsync_PatchesDecisionBody()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.SubmitDecisionAsync(5, MatchStatus.Rejected, "Lacking experience");

        handler.LastRequest!.Method.Should().Be(HttpMethod.Patch);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/matches/5/decision");
        var bodyJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        bodyDoc.RootElement.GetProperty("decision").GetString().Should().Be("Rejected");
        bodyDoc.RootElement.GetProperty("feedback").GetString().Should().Be("Lacking experience");
    }

    [Fact]
    public async Task AcceptAsync_PatchesAcceptedDecision()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.AcceptAsync(5, "Welcome");

        var bodyJson = await handler.LastRequest!.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/matches/5/decision");
        bodyDoc.RootElement.GetProperty("decision").GetString().Should().Be("Accepted");
        bodyDoc.RootElement.GetProperty("feedback").GetString().Should().Be("Welcome");
    }

    [Fact]
    public async Task RejectAsync_PatchesRejectedDecision()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.RejectAsync(6, "Not a fit");

        var bodyJson = await handler.LastRequest!.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/matches/6/decision");
        bodyDoc.RootElement.GetProperty("decision").GetString().Should().Be("Rejected");
        bodyDoc.RootElement.GetProperty("feedback").GetString().Should().Be("Not a fit");
    }

    [Fact]
    public async Task GetByIdAsync_Returns404_ReturnsNull()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NotFound, new StringContent(""));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetByIdAsync(404);

        result.Should().BeNull();
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/api/matches/404");
    }

    private static Match CreateMatch()
    {
        return new Match
        {
            MatchId = 1,
            User = new UserBuilder().WithId(11).Build(),
            Job = new JobBuilder().WithId(21).WithCompanyId(42).Build(),
            Status = MatchStatus.Advanced,
            Timestamp = DateTime.UtcNow,
        };
    }
}
