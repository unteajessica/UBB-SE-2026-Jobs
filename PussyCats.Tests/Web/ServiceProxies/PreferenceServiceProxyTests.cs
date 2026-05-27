using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.ServiceProxies;

namespace PussyCats.Tests.Web.ServiceProxies;

public class PreferenceServiceProxyTests
{
    private static PreferenceServiceProxy CreateProxy(StubHttpMessageHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });

    [Fact]
    public async Task GetByUserIdAsync_HitsExpectedPathAndDeserialisesEnumsAsStrings()
    {
        var prefsJson = JsonContent.Create(new
        {
            roles = new[] { "BackendDeveloper", "DataAnalyst" },
            workMode = "Hybrid",
            location = "Cluj-Napoca, Romania",
        });
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, prefsJson);
        var proxy = CreateProxy(handler);

        var result = await proxy.GetByUserIdAsync(7);

        handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/preferences/7");
        result.Roles.Should().BeEquivalentTo(new[] { JobRole.BackendDeveloper, JobRole.DataAnalyst });
        result.WorkMode.Should().Be(WorkMode.Hybrid);
        result.Location.Should().Be("Cluj-Napoca, Romania");
    }

    [Fact]
    public async Task SavePreferencesAsync_PutsRolesAsEnumStrings()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.NoContent, new StringContent(""));
        var proxy = CreateProxy(handler);

        await proxy.SavePreferencesAsync(
            7,
            new[] { JobRole.FrontendDeveloper },
            WorkMode.Remote,
            "Berlin, Germany");

        handler.LastRequest!.Method.Should().Be(HttpMethod.Put);
        handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/api/preferences/7");
        var body = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(body);
        bodyDoc.RootElement.GetProperty("Roles")[0].GetString().Should().Be("FrontendDeveloper");
        bodyDoc.RootElement.GetProperty("WorkMode").GetString().Should().Be("Remote");
        bodyDoc.RootElement.GetProperty("Location").GetString().Should().Be("Berlin, Germany");
    }

    [Fact]
    public async Task SearchLocationsAsync_UrlEncodesQuery()
    {
        var handler = StubHttpMessageHandler.Returning(
            HttpStatusCode.OK,
            JsonContent.Create(new[] { "Cluj-Napoca, Romania" }));
        var proxy = CreateProxy(handler);

        var result = await proxy.SearchLocationsAsync("Cluj Napoca");

        handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
        handler.LastRequest.RequestUri!.PathAndQuery.Should().Be("/api/preferences/locations?locationQuery=Cluj%20Napoca");
        result.Should().ContainSingle().Which.Should().Be("Cluj-Napoca, Romania");
    }
}
