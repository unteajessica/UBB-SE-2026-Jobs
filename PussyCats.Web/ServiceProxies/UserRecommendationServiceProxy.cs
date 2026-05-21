using PussyCats.Library.DTOs;
using PussyCats.Library.Services.UserRecommendationService;

namespace PussyCats.Web.ServiceProxies
{
    public class UserRecommendationServiceProxy : IUserRecommendationService
    {
        private readonly HttpClient httpClient;

        public UserRecommendationServiceProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<JobRecommendationResult?> GetNextCardAsync(int userId, UserMatchmakingFilters filters, CancellationToken cancellationToken = default)
        {
            // Post the filters and user context down to the backend Web API
            var response = await httpClient.PostAsJsonAsync($"api/recommendations/{userId}/next", filters, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<JobRecommendationResult>(cancellationToken: cancellationToken);
        }

        public async Task<JobRecommendationResult?> RecalculateTopCardIgnoringCooldownAsync(int userId, UserMatchmakingFilters filters, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync($"api/recommendations/{userId}/fallback", filters, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<JobRecommendationResult>(cancellationToken: cancellationToken);
        }

        public async Task<int> ApplyLikeAsync(int userId, JobRecommendationResult card, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync($"api/recommendations/{userId}/like", card, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task<int> ApplyDismissAsync(int userId, JobRecommendationResult card, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync($"api/recommendations/{userId}/dismiss", card, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task UndoLikeAsync(int matchId, int? displayRecommendationId, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsync($"api/recommendations/undo-like?matchId={matchId}&displayId={displayRecommendationId}", null, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task UndoDismissAsync(int dismissRecommendationId, int? displayRecommendationId, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsync($"api/recommendations/undo-dismiss?dismissId={dismissRecommendationId}&displayId={displayRecommendationId}", null, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
