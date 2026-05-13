using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats_App.Services.DeveloperService;

namespace PussyCats.App.ViewModels;

public class DeveloperViewModel : DispatchableObservableObject
{
    private readonly IDeveloperService developerService;
    private readonly SessionContext session;
    private DeveloperParameterOption selectedParameter;
    private string proposedValue = string.Empty;
    private string statusMessage = string.Empty;

    public DeveloperViewModel(IDeveloperService developerService, SessionContext session)
    {
        this.developerService = developerService;
        this.session = session;
        ParameterOptions =
        [
            new DeveloperParameterOption(DeveloperPostParameterType.WeightedDistanceScoreWeight),
            new DeveloperParameterOption(DeveloperPostParameterType.JobResumeSimilarityScoreWeight),
            new DeveloperParameterOption(DeveloperPostParameterType.PreferenceScoreWeight),
            new DeveloperParameterOption(DeveloperPostParameterType.PromotionScoreWeight),
            new DeveloperParameterOption(DeveloperPostParameterType.MitigationFactor),
            new DeveloperParameterOption(DeveloperPostParameterType.RelevantKeyword),
        ];
        selectedParameter = ParameterOptions[0];
        AddPostCommand = new RelayCommand(AddPost);
        RefreshCommand = new RelayCommand(RefreshPosts);
        RefreshPosts();
    }

    public ObservableCollection<DeveloperFeedPostCardViewModel> Posts { get; } = new();
    public IReadOnlyList<DeveloperParameterOption> ParameterOptions { get; }
    public ICommand AddPostCommand { get; }
    public ICommand RefreshCommand { get; }

    public DeveloperParameterOption SelectedParameter
    {
        get => selectedParameter;
        set => SetProperty(ref selectedParameter, value);
    }

    public string ProposedValue
    {
        get => proposedValue;
        set => SetProperty(ref proposedValue, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public void AddPost()
    {
        var validation = ValidateDeveloperPostInput(SelectedParameter.Type, ProposedValue);
        if (validation is not null)
        {
            StatusMessage = validation;
            return;
        }

        var developerId = session.DeveloperId ?? 1;
        developerService.AddPost(developerId, SelectedParameter.Type, ProposedValue);
        ProposedValue = string.Empty;
        StatusMessage = "Post published.";
        RefreshPosts();
    }

    public string? ValidateDeveloperPostInput(DeveloperPostParameterType parameter, string value)
    {
        if (parameter == DeveloperPostParameterType.RelevantKeyword)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Keyword cannot be empty.";
            }

            if (value != value.ToLowerInvariant())
            {
                return "Keyword must be lowercase.";
            }

            return null;
        }

        if (parameter == DeveloperPostParameterType.MitigationFactor)
        {
            return double.TryParse(value, out var factor) && factor >= 1
                ? null
                : "Mitigation factor must be a number greater than or equal to 1.";
        }

        return double.TryParse(value, out var weight) && weight is >= 0 and <= 100
            ? null
            : "Weight value must be a number between 0 and 100.";
    }

    public void RefreshPosts()
    {
        var posts = developerService.GetPosts();
        var interactions = developerService.GetInteractions();
        var currentDeveloperId = session.DeveloperId ?? 1;

        Posts.Clear();
        foreach (var post in posts)
        {
            var postInteractions = interactions.Where(interaction => interaction.DeveloperPost.DeveloperPostId == post.DeveloperPostId).ToList();
            var developer = developerService.GetDeveloperById(post.Developer.DeveloperId);
            Posts.Add(new DeveloperFeedPostCardViewModel(
                post,
                postInteractions,
                developer?.Name ?? $"Developer {post.Developer.DeveloperId}",
                currentDeveloperId,
                ToggleLike,
                ToggleDislike));
        }
    }

    private void ToggleLike(int postId)
    {
        ToggleInteraction(postId, DeveloperInteractionType.Like);
    }

    private void ToggleDislike(int postId)
    {
        ToggleInteraction(postId, DeveloperInteractionType.Dislike);
    }

    private void ToggleInteraction(int postId, DeveloperInteractionType type)
    {
        var developerId = session.DeveloperId ?? 1;
        var existing = developerService.GetInteractions()
            .FirstOrDefault(interaction => interaction.Developer.DeveloperId == developerId && interaction.DeveloperPost.DeveloperPostId == postId);
        if (existing is not null && existing.Type == type)
        {
            developerService.RemoveInteraction(existing.DeveloperInteractionId);
        }
        else
        {
            developerService.AddInteraction(developerId, postId, type);
        }

        RefreshPosts();
    }
}

public sealed record DeveloperParameterOption(DeveloperPostParameterType Type)
{
    public string DisplayName => DeveloperPostParameterTypeMapper.ToDisplayName(Type);
}

public sealed class DeveloperFeedPostCardViewModel
{
    public DeveloperFeedPostCardViewModel(
        DeveloperPost post,
        IReadOnlyList<DeveloperInteraction> interactions,
        string authorName,
        int currentDeveloperId,
        Action<int> likeAction,
        Action<int> dislikeAction)
    {
        PostId = post.DeveloperPostId;
        AuthorName = authorName;
        AuthorInitial = string.IsNullOrWhiteSpace(authorName) ? "D" : authorName[0].ToString().ToUpperInvariant();
        ParameterDisplayName = DeveloperPostParameterTypeMapper.ToDisplayName(post.ParameterType);
        ValueDisplay = post.Value;
        TypeLabel = post.ParameterType == DeveloperPostParameterType.RelevantKeyword ? "Keyword" : "Parameter";
        CreatedAt = post.CreatedAt.ToLocalTime().ToString("dd MMM HH:mm");
        LikeCount = interactions.Count(interaction => interaction.Type == DeveloperInteractionType.Like);
        DislikeCount = interactions.Count(interaction => interaction.Type == DeveloperInteractionType.Dislike);
        IsLikedByCurrentUser = interactions.Any(interaction =>
            interaction.Developer.DeveloperId == currentDeveloperId && interaction.Type == DeveloperInteractionType.Like);
        IsDislikedByCurrentUser = interactions.Any(interaction =>
            interaction.Developer.DeveloperId == currentDeveloperId && interaction.Type == DeveloperInteractionType.Dislike);
        LikeCommand = new RelayCommand(() => likeAction(PostId));
        DislikeCommand = new RelayCommand(() => dislikeAction(PostId));
    }

    public int PostId { get; }
    public string AuthorName { get; }
    public string AuthorInitial { get; }
    public string ParameterDisplayName { get; }
    public string ValueDisplay { get; }
    public string TypeLabel { get; }
    public string CreatedAt { get; }
    public int LikeCount { get; }
    public int DislikeCount { get; }
    public bool IsLikedByCurrentUser { get; }
    public bool IsDislikedByCurrentUser { get; }
    public ICommand LikeCommand { get; }
    public ICommand DislikeCommand { get; }
}
