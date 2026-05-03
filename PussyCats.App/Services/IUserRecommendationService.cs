using PussyCats.Library.DTOs;

namespace PussyCats.App.Services;

public interface IUserRecommendationService
{
    Task<int> ApplyDismissAsync(int userId, JobRecommendationResult card, CancellationToken ct = default);

    Task<int> ApplyLikeAsync(int userId, JobRecommendationResult card, CancellationToken ct = default);

    Task<JobRecommendationResult?> GetNextCardAsync(int userId, UserMatchmakingFilters filters, CancellationToken ct = default);

    Task<JobRecommendationResult?> RecalculateTopCardIgnoringCooldownAsync(int userId, UserMatchmakingFilters filters, CancellationToken ct = default);

    Task UndoDismissAsync(int dismissRecommendationId, int? displayRecommendationId, CancellationToken ct = default);

    Task UndoLikeAsync(int matchId, int? displayRecommendationId, CancellationToken ct = default);
}
