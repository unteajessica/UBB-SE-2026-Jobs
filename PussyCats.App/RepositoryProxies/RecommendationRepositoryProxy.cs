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

    public async Task<Recommendation?> GetByIdAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Recommendation>(
            http,
            $"api/recommendations/{recommendationId}",
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<Recommendation>(http, "api/recommendations", cancellationToken).ConfigureAwait(false);
    }

    public async Task<Recommendation?> GetLatestByUserIdAndJobIdAsync(int userId, int jobId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<Recommendation>(
            http,
            $"api/recommendations?userId={userId}&jobId={jobId}",
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<Recommendation> AddAsync(Recommendation recommendation, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            UserId = recommendation.User.UserId,
            JobId = recommendation.Job.JobId,
            recommendation.Timestamp,
        };

        using var response = await http.PostAsJsonAsync(
            "api/recommendations",
            request,
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<Recommendation>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Recommendation recommendation, CancellationToken cancellationToken = default)
    {
        var request = new { recommendation.Timestamp };
        using var response = await http.PutAsJsonAsync(
            $"api/recommendations/{recommendation.RecommendationId}",
            request,
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/recommendations/{recommendationId}", cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
