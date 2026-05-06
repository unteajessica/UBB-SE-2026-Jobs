using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Recommendations;

namespace PussyCats.App.RepositoryProxies;

public class RecommendationRepositoryProxy : IRecommendationRepository
{
    private readonly HttpClient http;

    public RecommendationRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Recommendation>(
            http,
            $"api/recommendations/{recommendationId}",
            ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<Recommendation>(http, "api/recommendations", ct).ConfigureAwait(false);
    }

    public async Task<Recommendation?> GetLatestByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Recommendation>(
            http,
            $"api/recommendations?userId={userId}&jobId={jobId}",
            ct).ConfigureAwait(false);
    }

    public async Task<Recommendation> AddAsync(Recommendation recommendation, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync(
            "api/recommendations",
            recommendation,
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Recommendation>(response, ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int recommendationId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/recommendations/{recommendationId}", ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
