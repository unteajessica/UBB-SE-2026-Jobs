using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Preferences;

namespace PussyCats.Library.ServiceProxies;

public class PreferenceServiceProxy : IPreferenceService
{
    private readonly HttpClient http;

    // API serialises enums as strings (JobRole, WorkMode); default HttpClient options expect
    // numeric enums and would fail on round-trip. Centralise once for this proxy.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public PreferenceServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<UserPreferences> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var preferences = await http
            .GetFromJsonAsync<UserPreferences>($"api/preferences/{userId}", JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return preferences ?? new UserPreferences([], default, string.Empty);
    }

    public async Task SavePreferencesAsync(
        int userId,
        IReadOnlyList<JobRole> roles,
        WorkMode workMode,
        string location,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            Roles = roles,
            WorkMode = workMode,
            Location = location ?? string.Empty,
        };
        var response = await http
            .PutAsJsonAsync($"api/preferences/{userId}", body, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<string>> SearchLocationsAsync(string locationQuery, CancellationToken cancellationToken = default)
    {
        var results = await http
            .GetFromJsonAsync<List<string>>($"api/preferences/locations?locationQuery={Uri.EscapeDataString(locationQuery ?? string.Empty)}", JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return results ?? [];
    }
}
