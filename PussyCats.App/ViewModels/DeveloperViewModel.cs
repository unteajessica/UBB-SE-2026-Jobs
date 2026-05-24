using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Developers;

namespace PussyCats.App.ViewModels;

public class DeveloperViewModel : DispatchableObservableObject
{
    private readonly IDeveloperService developerService;
    private readonly SessionContext session;
    private DeveloperParameterOption selectedParameter;
    private string proposedValue = string.Empty;
    private string statusMessage = string.Empty;
    private bool isInitialized = false;

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
        AddPostCommand = new AsyncRelayCommand(AddPostAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshPostsAsync);
    }

    public async Task InitializeAsync()
    {
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;

        await RefreshPostsAsync();
    }

    public ObservableCollection<DeveloperFeedPostCardViewModel> Posts { get; } = new();
    public IReadOnlyList<DeveloperParameterOption> ParameterOptions { get; }
    public IAsyncRelayCommand AddPostCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

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

    public async Task AddPostAsync()
    {
        var validation = ValidateDeveloperPostInput(SelectedParameter.Type, ProposedValue);
        if (validation is not null)
        {
            StatusMessage = validation;
            return;
        }

        var developerId = session.DeveloperId ?? 1;
        await developerService.AddPostAsync(developerId, SelectedParameter.Type, ProposedValue);
        ProposedValue = string.Empty;
        StatusMessage = "Post published.";
        await RefreshPostsAsync();
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

    public async Task RefreshPostsAsync()
    {
        var posts = await developerService.GetPostsAsync();
        var interactions = await developerService.GetInteractionsAsync();
        var currentDeveloperId = session.DeveloperId ?? 1;

        Posts.Clear();
        /// This is dangerous because of N+1 api calls, but it's a demo app and we don't expect many posts or interactions, so it should be fine for now.
        /// In a real app, we'd want to optimize this by including developer info in the GetPostsAsync response or caching developer info.
        foreach (var post in posts)
        {
            var postInteractions = interactions.Where(interaction => interaction.DeveloperPost.DeveloperPostId == post.DeveloperPostId).ToList();
            var developer = await developerService.GetDeveloperByIdAsync(post.Developer.DeveloperId);
            Posts.Add(new DeveloperFeedPostCardViewModel(
                post,
                postInteractions,
                developer?.Name ?? $"Developer {post.Developer.DeveloperId}",
                currentDeveloperId,
                ToggleLike,
                ToggleDislike));
        }
    }

    private async Task ToggleLike(int postId)
    {
        await ToggleInteraction(postId, DeveloperInteractionType.Like);
    }

    private async Task ToggleDislike(int postId)
    {
        await ToggleInteraction(postId, DeveloperInteractionType.Dislike);
    }

    private async Task ToggleInteraction(int postId, DeveloperInteractionType type)
    {
        var developerId = session.DeveloperId ?? 1;
        var existing = (await developerService.GetInteractionsAsync())
            .FirstOrDefault(interaction => interaction.Developer.DeveloperId == developerId && interaction.DeveloperPost.DeveloperPostId == postId);
        if (existing is not null && existing.Type == type)
        {
            await developerService.RemoveInteractionAsync(existing.DeveloperInteractionId);
        }
        else
        {
            await developerService.AddInteractionAsync(developerId, postId, type);
        }

        await RefreshPostsAsync();
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
        Func<int, Task> likeAction,
        Func<int, Task> dislikeAction)
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
