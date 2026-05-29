using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Helpers;
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

        result.Should().HaveCount(1);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest.RequestUri!.PathAndQuery.Should()
            .Be("/api/company-recommendations/companies/42/applicants");
    }

    [Fact]
    public async Task GetApplicantByMatchIdAsync_Returns404_ReturnsNull()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NotFound, new StringContent(""));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetApplicantByMatchIdAsync(42, 7);

        result.Should().BeNull();
        handler.LastRequest!.RequestUri!.PathAndQuery.Should()
            .Be("/api/company-recommendations/companies/42/applicants/7");
    }

    [Fact]
    public async Task LoadApplicantsAsync_StoresApplicantsForQueueNavigation()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, JsonContent.Create(new[] { BuildApplicant(7) }));
        var proxy = CreateProxy(handler);

        await proxy.LoadApplicantsAsync(42);

        proxy.HasMore.Should().BeTrue();
        proxy.GetNextApplicant()!.Match.MatchId.Should().Be(7);
        proxy.MoveToNext();
        proxy.HasMore.Should().BeFalse();
    }

    [Fact]
    public async Task GetBreakdownAsync_PostsApplicantBody()
    {
        var handler = StubHttpMessageHandler.Returning(
            HttpStatusCode.OK,
            JsonContent.Create(new CompatibilityBreakdown { OverallScore = 91 }));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetBreakdownAsync(BuildApplicant(7));

        result!.OverallScore.Should().Be(91);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/company-recommendations/breakdown");
        var bodyJson = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(bodyJson);
        bodyDoc.RootElement.GetProperty("match").GetProperty("matchId").GetInt32().Should().Be(7);
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
