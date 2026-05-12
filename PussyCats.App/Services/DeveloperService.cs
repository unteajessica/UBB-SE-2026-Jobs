using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.Services;

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
        AddSeedPost(2, DeveloperPostParameterType.WeightedDistanceScoreWeight, "35");
        AddSeedPost(3, DeveloperPostParameterType.RelevantKeyword, "internship");
        AddSeedPost(1, DeveloperPostParameterType.MitigationFactor, "2.5");
        AddInteraction(1, 1, DeveloperInteractionType.Like);
        AddInteraction(2, 2, DeveloperInteractionType.Like);
        AddInteraction(3, 1, DeveloperInteractionType.Dislike);
    }

    public IReadOnlyList<DeveloperPost> GetPosts()
    {
        return posts
            .OrderByDescending(post => post.CreatedAt)
            .Select(ClonePost)
            .ToList();
    }

    public IReadOnlyList<DeveloperInteraction> GetInteractions()
    {
        return interactions.Select(CloneInteraction).ToList();
    }

    public Developer? GetDeveloperById(int developerId)
    {
        var developer = developers.FirstOrDefault(developer => developer.DeveloperId == developerId);
        return developer is null
            ? null
            : new Developer { DeveloperId = developer.DeveloperId, Name = developer.Name };
    }

    public DeveloperPost AddPost(int developerId, DeveloperPostParameterType parameterType, string value)
    {
        if (parameterType == DeveloperPostParameterType.Unknown)
        {
            throw new ArgumentException("Choose a valid parameter.", nameof(parameterType));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", nameof(value));
        }

        var post = new DeveloperPost
        {
            DeveloperPostId = nextPostId++,
            Developer = new Developer { DeveloperId = developerId },
            ParameterType = parameterType,
            Value = value.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
        posts.Add(post);
        return ClonePost(post);
    }

    public void AddInteraction(int developerId, int postId, DeveloperInteractionType type)
    {
        var existing = interactions.FirstOrDefault(interaction =>
            interaction.Developer.DeveloperId == developerId && interaction.DeveloperPost.DeveloperPostId == postId);
        if (existing is not null)
        {
            existing.Type = type;
            return;
        }

        interactions.Add(new DeveloperInteraction
        {
            DeveloperInteractionId = nextInteractionId++,
            Developer = new Developer { DeveloperId = developerId },
            DeveloperPost = new DeveloperPost { DeveloperPostId = postId },
            Type = type,
        });
    }

    public void RemoveInteraction(int interactionId)
    {
        var existing = interactions.FirstOrDefault(interaction => interaction.DeveloperInteractionId == interactionId);
        if (existing is not null)
        {
            interactions.Remove(existing);
        }
    }

    private void AddSeedPost(int developerId, DeveloperPostParameterType parameterType, string value)
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

    private static DeveloperPost ClonePost(DeveloperPost post)
    {
        return new DeveloperPost
        {
            DeveloperPostId = post.DeveloperPostId,
            Developer = post.Developer,
            ParameterType = post.ParameterType,
            Value = post.Value,
            CreatedAt = post.CreatedAt,
        };
    }

    private static DeveloperInteraction CloneInteraction(DeveloperInteraction interaction)
    {
        return new DeveloperInteraction
        {
            DeveloperInteractionId = interaction.DeveloperInteractionId,
            Developer = interaction.Developer,
            DeveloperPost = interaction.DeveloperPost,
            Type = interaction.Type,
        };
    }
}
