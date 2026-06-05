using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Fakes;
using PussyCats.Library.ServiceProxies;

namespace PussyCats.Tests.Web.ServiceProxies;

public class CompanyRecommendationServiceProxyTests
{
    private static CompanyRecommendationServiceProxy CreateProxy(StubHttpMessageHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

    [Fact]
    public async Task GetRankedApplicantsAsync_BuildsCompanyRoute()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, JsonContent.Create(new[] { BuildApplicant(7) }));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetRankedApplicantsAsync(42);

        Assert.Equal(1, result.Count());
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Equal("/api/company-recommendations/companies/42/applicants", handler.LastRequest.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task GetApplicantByMatchIdAsync_Returns404_ReturnsNull()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NotFound, new StringContent(""));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetApplicantByMatchIdAsync(42, 7);

        Assert.Null(result);
        Assert.Equal("/api/company-recommendations/companies/42/applicants/7", handler.LastRequest!.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task LoadApplicantsAsync_StoresApplicantsForQueueNavigation()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, JsonContent.Create(new[] { BuildApplicant(7) }));
        var proxy = CreateProxy(handler);

        await proxy.LoadApplicantsAsync(42);

        Assert.True(proxy.HasMore);
        Assert.Equal(7, proxy.GetNextApplicant()!.Match.MatchId);
        proxy.MoveToNext();
        Assert.False(proxy.HasMore);
    }

    [Fact]
    public async Task GetBreakdownAsync_PostsApplicantBody()
    {
        var handler = StubHttpMessageHandler.Returning(
            HttpStatusCode.OK,
            JsonContent.Create(new CompatibilityBreakdown { OverallScore = 91 }));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetBreakdownAsync(BuildApplicant(7));

        Assert.Equal(91, result!.OverallScore);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("/api/company-recommendations/breakdown", handler.LastRequest.RequestUri!.AbsolutePath);
        var bodyJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        Assert.Equal(7, bodyDoc.RootElement.GetProperty("match").GetProperty("matchId").GetInt32());
    }

    private static UserApplicationResult BuildApplicant(int matchId)
    {
        var user = new UserBuilder().WithId(11).Build();
        var job = new JobBuilder().WithId(21).WithCompanyId(42).Build();

        return new UserApplicationResult
        {
            User = user,
            Match = new Match
            {
                MatchId = matchId,
                User = user,
                Job = job,
                Timestamp = DateTime.UtcNow,
            },
            Job = job,
            CompatibilityScore = 88,
            UserSkills = new List<UserSkill>(),
        };
    }
}
