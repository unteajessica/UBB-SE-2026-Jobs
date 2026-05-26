using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Services.Developers;

public sealed class DeveloperService : IDeveloperService
{
    private readonly List<Developer> developers =
    [
        new Developer { DeveloperId = 1, Name = "Andrew" },
        new Developer { DeveloperId = 2, Name = "Varis" },
        new Developer { DeveloperId = 3, Name = "Clavicular" },
    ];

    private readonly List<DeveloperPost> posts = new();
    private readonly List<DeveloperInteraction> interactions = new();
    private int nextPostId = 1;
    private int nextInteractionId = 1;

    public DeveloperService()
    {
        SeedPost(2, DeveloperPostParameterType.WeightedDistanceScoreWeight, "35");
        SeedPost(3, DeveloperPostParameterType.RelevantKeyword, "internship");
        SeedPost(1, DeveloperPostParameterType.MitigationFactor, "2.5");
        SeedInteraction(1, 1, DeveloperInteractionType.Like);
        SeedInteraction(2, 2, DeveloperInteractionType.Like);
        SeedInteraction(3, 1, DeveloperInteractionType.Dislike);
    }

    public Task<IReadOnlyList<DeveloperPost>> GetPostsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DeveloperPost> result = posts
            .OrderByDescending(post => post.CreatedAt)
            .Select(ClonePost)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<DeveloperInteraction>> GetInteractionsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DeveloperInteraction> result = interactions.Select(CloneInteraction).ToList();
        return Task.FromResult(result);
    }

    public Task<Developer?> GetDeveloperByIdAsync(int developerId, CancellationToken cancellationToken = default)
    {
        var developer = developers.FirstOrDefault(developerToCheck => developerToCheck.DeveloperId == developerId);
        Developer? result = developer is null ? null : new Developer { DeveloperId = developer.DeveloperId, Name = developer.Name };
        return Task.FromResult(result);
    }

    public Task<DeveloperPost> AddPostAsync(int developerId, DeveloperPostParameterType parameterType, string value, CancellationToken cancellationToken = default)
    {
        if (parameterType == DeveloperPostParameterType.Unknown)
            throw new ArgumentException("Choose a valid parameter.", nameof(parameterType));
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty.", nameof(value));

        var post = new DeveloperPost
        {
            DeveloperPostId = nextPostId++,
            Developer = new Developer { DeveloperId = developerId },
            ParameterType = parameterType,
            Value = value.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
        posts.Add(post);
        return Task.FromResult(ClonePost(post));
    }

    public Task AddInteractionAsync(int developerId, int postId, DeveloperInteractionType type, CancellationToken cancellationToken = default)
    {
        var existing = interactions.FirstOrDefault(interactionToCheckIfExists =>
            interactionToCheckIfExists.Developer.DeveloperId == developerId && interactionToCheckIfExists.DeveloperPost.DeveloperPostId == postId);
        if (existing is not null)
        {
            existing.Type = type;
            return Task.CompletedTask;
        }

        interactions.Add(new DeveloperInteraction
        {
            DeveloperInteractionId = nextInteractionId++,
            Developer = new Developer { DeveloperId = developerId },
            DeveloperPost = new DeveloperPost { DeveloperPostId = postId },
            Type = type,
        });
        return Task.CompletedTask;
    }

    public Task RemoveInteractionAsync(int interactionId, CancellationToken cancellationToken = default)
    {
        var existing = interactions.FirstOrDefault(interactionToCheckIfExists => interactionToCheckIfExists.DeveloperInteractionId == interactionId);
        if (existing is not null)
            interactions.Remove(existing);
        return Task.CompletedTask;
    }

    private void SeedPost(int developerId, DeveloperPostParameterType parameterType, string value)
    {
        posts.Add(new DeveloperPost
        {
            DeveloperPostId = nextPostId++,
            Developer = new Developer { DeveloperId = developerId },
            ParameterType = parameterType,
            Value = value,
            CreatedAt = DateTime.UtcNow.AddMinutes(-nextPostId * 12),
        });
    }

    private void SeedInteraction(int developerId, int postId, DeveloperInteractionType type)
    {
        interactions.Add(new DeveloperInteraction
        {
            DeveloperInteractionId = nextInteractionId++,
            Developer = new Developer { DeveloperId = developerId },
            DeveloperPost = new DeveloperPost { DeveloperPostId = postId },
            Type = type,
        });
    }

    private static DeveloperPost ClonePost(DeveloperPost post) => new()
    {
        DeveloperPostId = post.DeveloperPostId,
        Developer = post.Developer,
        ParameterType = post.ParameterType,
        Value = post.Value,
        CreatedAt = post.CreatedAt,
    };

    private static DeveloperInteraction CloneInteraction(DeveloperInteraction interaction) => new()
    {
        DeveloperInteractionId = interaction.DeveloperInteractionId,
        Developer = interaction.Developer,
        DeveloperPost = interaction.DeveloperPost,
        Type = interaction.Type,
    };
}
