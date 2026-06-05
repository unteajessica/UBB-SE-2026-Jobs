using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Tests.Fakes;
using PussyCats.Library.ServiceProxies;

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

        Assert.Equal(1, result.Count());
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Equal("/api/matches?companyId=42", handler.LastRequest.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task SubmitDecisionAsync_PatchesDecisionBody()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.SubmitDecisionAsync(5, MatchStatus.Rejected, "Lacking experience");

        Assert.Equal(HttpMethod.Patch, handler.LastRequest!.Method);
        Assert.Equal("/api/matches/5/decision", handler.LastRequest.RequestUri!.AbsolutePath);
        var bodyJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        Assert.Equal("Rejected", bodyDoc.RootElement.GetProperty("decision").GetString());
        Assert.Equal("Lacking experience", bodyDoc.RootElement.GetProperty("feedback").GetString());
    }

    [Fact]
    public async Task AcceptAsync_PatchesAcceptedDecision()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.AcceptAsync(5, "Welcome");

        var bodyJson = await handler.LastRequest!.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        Assert.Equal("/api/matches/5/decision", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("Accepted", bodyDoc.RootElement.GetProperty("decision").GetString());
        Assert.Equal("Welcome", bodyDoc.RootElement.GetProperty("feedback").GetString());
    }

    [Fact]
    public async Task RejectAsync_PatchesRejectedDecision()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.RejectAsync(6, "Not a fit");

        var bodyJson = await handler.LastRequest!.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        Assert.Equal("/api/matches/6/decision", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("Rejected", bodyDoc.RootElement.GetProperty("decision").GetString());
        Assert.Equal("Not a fit", bodyDoc.RootElement.GetProperty("feedback").GetString());
    }

    [Fact]
    public async Task GetByIdAsync_Returns404_ReturnsNull()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NotFound, new StringContent(""));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetByIdAsync(404);

        Assert.Null(result);
        Assert.Equal("/api/matches/404", handler.LastRequest!.RequestUri!.AbsolutePath);
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
