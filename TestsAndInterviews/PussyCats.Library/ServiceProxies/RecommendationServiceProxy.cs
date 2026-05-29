using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Recommendations;

namespace PussyCats.Library.ServiceProxies;

public class RecommendationServiceProxy : IRecommendationService
{
    private readonly HttpClient http;

    public RecommendationServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<Recommendation>>("api/recommendations", cancellationToken) ?? new List<Recommendation>();

    public async Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/recommendations/{recommendationId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Recommendation>(cancellationToken: cancellationToken);
    }

    public async Task<Recommendation?> GetLatestForUserAndJobAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/recommendations?userId={userId}&jobId={jobId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Recommendation>(cancellationToken: cancellationToken);
    }

    public async Task<Recommendation> AddAsync(int userId, int jobId, DateTime? timestamp, CancellationToken cancellationToken = default)
    {
        var body = new { userId, jobId, timestamp = timestamp ?? default };
        var response = await http.PostAsJsonAsync("api/recommendations", body, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Recommendation>(cancellationToken: cancellationToken))!;
    }

    public async Task UpdateTimestampAsync(int recommendationId, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/recommendations/{recommendationId}", new { timestamp }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/recommendations/{recommendationId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
