using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Services.Developers;

public interface IDeveloperService
{
    Task<IReadOnlyList<DeveloperPost>> GetPostsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeveloperInteraction>> GetInteractionsAsync(CancellationToken cancellationToken = default);

    Task<Developer?> GetDeveloperByIdAsync(int developerId, CancellationToken cancellationToken = default);

    Task<DeveloperPost> AddPostAsync(int developerId, DeveloperPostParameterType parameterType, string value, CancellationToken cancellationToken = default);

    Task AddInteractionAsync(int developerId, int postId, DeveloperInteractionType type, CancellationToken cancellationToken = default);

    Task RemoveInteractionAsync(int interactionId, CancellationToken cancellationToken = default);
}
