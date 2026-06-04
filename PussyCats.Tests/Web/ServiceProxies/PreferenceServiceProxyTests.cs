using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Equal("/api/preferences/7", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal(new[] { JobRole.BackendDeveloper, JobRole.DataAnalyst }, result.Roles);
        Assert.Equal(WorkMode.Hybrid, result.WorkMode);
        Assert.Equal("Cluj-Napoca, Romania", result.Location);
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

        Assert.Equal(HttpMethod.Put, handler.LastRequest!.Method);
        Assert.Equal("/api/preferences/7", handler.LastRequest.RequestUri!.AbsolutePath);
        var body = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var bodyDoc = JsonDocument.Parse(body);
        Assert.Equal("FrontendDeveloper", bodyDoc.RootElement.GetProperty("Roles")[0].GetString());
        Assert.Equal("Remote", bodyDoc.RootElement.GetProperty("WorkMode").GetString());
        Assert.Equal("Berlin, Germany", bodyDoc.RootElement.GetProperty("Location").GetString());
    }

    [Fact]
    public async Task SearchLocationsAsync_UrlEncodesQuery()
    {
        var handler = StubHttpMessageHandler.Returning(
            HttpStatusCode.OK,
            JsonContent.Create(new[] { "Cluj-Napoca, Romania" }));
        var proxy = CreateProxy(handler);

        var result = await proxy.SearchLocationsAsync("Cluj Napoca");

        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Equal("/api/preferences/locations?locationQuery=Cluj%20Napoca", handler.LastRequest.RequestUri!.PathAndQuery);
        Assert.Equal("Cluj-Napoca, Romania", Assert.Single(result));
    }
}
