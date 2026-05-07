using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PussyCats.App.RepositoryProxies;

internal static class RepositoryProxyJson
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    public static async Task<T?> GetOrNullAsync<T>(HttpClient http, string uri, CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await ReadJsonOrDefaultAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<IReadOnlyList<T>> GetListAsync<T>(HttpClient http, string uri, CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.NoContent)
        {
            return Array.Empty<T>();
        }

        response.EnsureSuccessStatusCode();
        var values = await ReadJsonOrDefaultAsync<List<T>>(response, cancellationToken).ConfigureAwait(false);
        return values is null ? Array.Empty<T>() : values;
    }

    public static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();
        var value = await ReadJsonOrDefaultAsync<T>(response, cancellationToken).ConfigureAwait(false);
        return value ?? throw new InvalidOperationException($"The API returned an empty {typeof(T).Name} response.");
    }

    private static async Task<T?> ReadJsonOrDefaultAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength is 0)
        {
            return default;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        if (stream.CanSeek && stream.Length == 0)
        {
            return default;
        }

        try
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, Options, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException) when (!stream.CanSeek)
        {
            return default;
        }
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
