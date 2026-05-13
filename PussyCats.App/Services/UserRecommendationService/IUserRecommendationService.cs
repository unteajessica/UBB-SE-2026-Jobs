using PussyCats.Library.DTOs;

namespace PussyCats_App.Services.UserRecommendationService;

public interface IUserRecommendationService
{
    Task<int> ApplyDismissAsync(int userId, JobRecommendationResult card, CancellationToken cancellationToken = default);

    Task<int> ApplyLikeAsync(int userId, JobRecommendationResult card, CancellationToken cancellationToken = default);

    Task<JobRecommendationResult?> GetNextCardAsync(int userId, UserMatchmakingFilters filters, CancellationToken cancellationToken = default);

    Task<JobRecommendationResult?> RecalculateTopCardIgnoringCooldownAsync(int userId, UserMatchmakingFilters filters, CancellationToken cancellationToken = default);

    Task UndoDismissAsync(int dismissRecommendationId, int? displayRecommendationId, CancellationToken cancellationToken = default);

    Task UndoLikeAsync(int matchId, int? displayRecommendationId, CancellationToken cancellationToken = default);
}
