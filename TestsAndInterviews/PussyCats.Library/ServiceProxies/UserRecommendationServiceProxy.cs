using System.Text.Json;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.UserRecommendationService;

namespace PussyCats.Library.ServiceProxies
{
    public class UserRecommendationServiceProxy : IUserRecommendationService
    {
        private readonly HttpClient httpClient;

        // API serialises enums as strings (JsonStringEnumConverter, registered globally on the
        // API per docs/CLAUDE.md). HttpClient defaults expect enums as numbers, so we need this
        // explicitly on both serialisation directions.
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };

        public UserRecommendationServiceProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<JobRecommendationResult?> GetNextCardAsync(int userId, UserMatchmakingFilters filters, CancellationToken cancellationToken = default)
        {
            // Post the filters and user context down to the backend Web API
            var response = await httpClient.PostAsJsonAsync($"api/recommendations/{userId}/next", filters, JsonOptions, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<JobRecommendationResult>(JsonOptions, cancellationToken);
        }

        public async Task<JobRecommendationResult?> RecalculateTopCardIgnoringCooldownAsync(int userId, UserMatchmakingFilters filters, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync($"api/recommendations/{userId}/fallback", filters, JsonOptions, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<JobRecommendationResult>(JsonOptions, cancellationToken);
        }

        public async Task<int> ApplyLikeAsync(int userId, JobRecommendationResult card, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync($"api/recommendations/{userId}/like", card, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>(JsonOptions, cancellationToken);
        }

        public async Task<int> ApplyDismissAsync(int userId, JobRecommendationResult card, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync($"api/recommendations/{userId}/dismiss", card, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>(JsonOptions, cancellationToken);
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
