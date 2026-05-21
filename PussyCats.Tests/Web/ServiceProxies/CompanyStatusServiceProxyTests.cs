using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Helpers;
using PussyCats.Web.ServiceProxies;

namespace PussyCats.Tests.Web.ServiceProxies;

public class CompanyStatusServiceProxyTests
{
    private static CompanyStatusServiceProxy CreateProxy(StubHttpMessageHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

    [Fact]
    public async Task GetApplicantsForCompanyAsync_BuildsCompanyRoute()
    {
        var payload = new[] { BuildApplicant() };
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, JsonContent.Create(payload));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetApplicantsForCompanyAsync(42);

        result.Should().HaveCount(1);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be("/api/company-status/companies/42/applicants");
    }

    [Fact]
    public async Task GetApplicantByMatchIdAsync_Returns404_ReturnsNull()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NotFound, new StringContent(""));
        var proxy = CreateProxy(handler);

        var result = await proxy.GetApplicantByMatchIdAsync(42, 7);

        result.Should().BeNull();
        handler.LastRequest!.RequestUri!.PathAndQuery.Should().Be("/api/company-status/companies/42/applicants/7");
    }

    private static UserApplicationResult BuildApplicant()
    {
        return new UserApplicationResult
        {
            User = new UserBuilder().WithId(11).Build(),
            Match = new MatchBuilder().WithId(7).Build(),
            Job = new JobBuilder().WithId(21).WithCompanyId(42).Build(),
            CompatibilityScore = 90,
        };
    }
}
