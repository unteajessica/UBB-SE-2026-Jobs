using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.Services;

public interface IDeveloperService
{
    IReadOnlyList<DeveloperPost> GetPosts();

    IReadOnlyList<DeveloperInteraction> GetInteractions();

    Developer? GetDeveloperById(int developerId);

    DeveloperPost AddPost(int developerId, DeveloperPostParameterType parameterType, string value);

    void AddInteraction(int developerId, int postId, DeveloperInteractionType type);

    void RemoveInteraction(int interactionId);
}
