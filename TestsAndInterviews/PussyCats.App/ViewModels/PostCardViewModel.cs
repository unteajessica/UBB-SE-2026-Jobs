using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace PussyCats.App.ViewModels;

public class PostCardViewModel
{
    private readonly Action<int>? likePost;
    private readonly Action<int>? dislikePost;

    public PostCardViewModel()
        : this(0, string.Empty, string.Empty, string.Empty, string.Empty, 0, 0, false, false, null, null)
    {
    }

    public PostCardViewModel(
        int postId,
        string authorName,
        string typeLabel,
        string parameterDisplayName,
        string valueDisplay,
        int likeCount,
        int dislikeCount,
        bool isLikedByCurrentUser,
        bool isDislikedByCurrentUser,
        Action<int>? likePost,
        Action<int>? dislikePost)
    {
        this.likePost = likePost;
        this.dislikePost = dislikePost;

        PostId = postId;
        AuthorName = authorName;
        AuthorInitial = authorName.Length > 0 ? authorName[0].ToString().ToUpperInvariant() : "?";
        TypeLabel = typeLabel;
        IsKeyword = typeLabel.Equals("Keyword", StringComparison.OrdinalIgnoreCase);
        ParameterDisplayName = parameterDisplayName;
        ValueDisplay = valueDisplay;
        LikeCount = likeCount;
        DislikeCount = dislikeCount;
        IsLikedByCurrentUser = isLikedByCurrentUser;
        IsDislikedByCurrentUser = isDislikedByCurrentUser;
        LikeCommand = new RelayCommand(ExecuteLikePost);
        DislikeCommand = new RelayCommand(ExecuteDislikePost);
    }

    public int PostId { get; }
    public string AuthorName { get; }
    public string AuthorInitial { get; }
    public string TypeLabel { get; }
    public bool IsKeyword { get; }
    public string ParameterDisplayName { get; }
    public string ValueDisplay { get; }
    public int LikeCount { get; }
    public int DislikeCount { get; }
    public bool IsLikedByCurrentUser { get; }
    public bool IsDislikedByCurrentUser { get; }
    public ICommand LikeCommand { get; }
    public ICommand DislikeCommand { get; }

    private void ExecuteLikePost() => likePost?.Invoke(PostId);
    private void ExecuteDislikePost() => dislikePost?.Invoke(PostId);
}
