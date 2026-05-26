using System.ComponentModel.DataAnnotations;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Web.Models;

public class DeveloperPostViewModel
{
    public DeveloperPostViewModel(DeveloperPost post, IReadOnlyList<DeveloperInteraction> interactions, int currentUserId)
    {
        PostId = post.DeveloperPostId;
        AuthorName = post.Developer?.Name ?? $"Developer {post.Developer?.DeveloperId}";
        AuthorInitial = string.IsNullOrWhiteSpace(AuthorName) ? "D" : AuthorName[0].ToString().ToUpperInvariant();
        ParameterDisplayName = DeveloperPostParameterTypeMapper.ToDisplayName(post.ParameterType);
        TypeLabel = post.ParameterType == DeveloperPostParameterType.RelevantKeyword ? "Keyword" : "Parameter";
        Value = post.Value;
        CreatedAt = post.CreatedAt.ToLocalTime().ToString("dd MMM HH:mm");
        LikeCount = interactions.Count(interactionsToGetLikesFrom => interactionsToGetLikesFrom.Type == DeveloperInteractionType.Like);
        DislikeCount = interactions.Count(interactionsToGetDislikesFrom => interactionsToGetDislikesFrom.Type == DeveloperInteractionType.Dislike);
        IsLikedByCurrentUser = interactions.Any(interactionsToCheckIfLiked => 
            interactionsToCheckIfLiked.Developer.DeveloperId == currentUserId && interactionsToCheckIfLiked.Type == DeveloperInteractionType.Like);
        IsDislikedByCurrentUser = interactions.Any(interactionsToCheckIfDisliked => 
            interactionsToCheckIfDisliked.Developer.DeveloperId == currentUserId && interactionsToCheckIfDisliked.Type == DeveloperInteractionType.Dislike);
    }

    public int PostId { get; }
    public string AuthorName { get; }
    public string AuthorInitial { get; }
    public string ParameterDisplayName { get; }
    public string TypeLabel { get; }
    public string Value { get; }
    public string CreatedAt { get; }
    public int LikeCount { get; }
    public int DislikeCount { get; }
    public bool IsLikedByCurrentUser { get; }
    public bool IsDislikedByCurrentUser { get; }
}

public class DeveloperPostCreateModel
{
    [Required]
    public DeveloperPostParameterType ParameterType { get; set; }

    [Required, MaxLength(200)]
    public string Value { get; set; } = string.Empty;
}
