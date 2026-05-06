using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PussyCats.App.RepositoryProxies;

internal static class RepositoryProxyJson
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    public static async Task<T?> GetOrNullAsync<T>(HttpClient http, string uri, CancellationToken ct)
    {
        using var response = await http.GetAsync(uri, ct).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(Options, ct).ConfigureAwait(false);
    }

    public static async Task<IReadOnlyList<T>> GetListAsync<T>(HttpClient http, string uri, CancellationToken ct)
    {
        using var response = await http.GetAsync(uri, ct).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Array.Empty<T>();
        }

        response.EnsureSuccessStatusCode();
        var values = await response.Content.ReadFromJsonAsync<List<T>>(Options, ct).ConfigureAwait(false);
        return values is null ? Array.Empty<T>() : values;
    }

    public static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        response.EnsureSuccessStatusCode();
        var value = await response.Content.ReadFromJsonAsync<T>(Options, ct).ConfigureAwait(false);
        return value ?? throw new InvalidOperationException($"The API returned an empty {typeof(T).Name} response.");
    }

    public static Task SendAndIgnoreNotFoundAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Task.CompletedTask;
        }

        response.EnsureSuccessStatusCode();
        return Task.CompletedTask;
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };

        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
