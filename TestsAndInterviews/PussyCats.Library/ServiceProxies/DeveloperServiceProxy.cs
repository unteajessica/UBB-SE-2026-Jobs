using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Developers;

namespace PussyCats.Library.ServiceProxies;

public class DeveloperServiceProxy : IDeveloperService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient http;

    public DeveloperServiceProxy(HttpClient http) => this.http = http;

    public async Task<IReadOnlyList<DeveloperPost>> GetPostsAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<DeveloperPost>>("api/developer/posts", JsonOptions, cancellationToken)
           ?? new List<DeveloperPost>();

    public async Task<IReadOnlyList<DeveloperInteraction>> GetInteractionsAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<DeveloperInteraction>>("api/developer/interactions", JsonOptions, cancellationToken)
           ?? new List<DeveloperInteraction>();

    public async Task<Developer?> GetDeveloperByIdAsync(int developerId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/developer/developers/{developerId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Developer>(JsonOptions, cancellationToken: cancellationToken);
    }

    public async Task<DeveloperPost> AddPostAsync(int developerId, DeveloperPostParameterType parameterType, string value, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/developer/posts",
            new { developerId, parameterType, value }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DeveloperPost>(JsonOptions, cancellationToken: cancellationToken))!;
    }

    public async Task AddInteractionAsync(int developerId, int postId, DeveloperInteractionType type, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/developer/interactions",
            new { developerId, postId, type }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveInteractionAsync(int interactionId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/developer/interactions/{interactionId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
