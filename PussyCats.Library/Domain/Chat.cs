namespace PussyCats.Library.Domain;
using System.Text.Json.Serialization;

public class Chat
{
    public int ChatId { get; set; }
    public User User { get; set; } = null!;
    public Company? Company { get; set; }
    public int? SecondUserId { get; set; }
    public User? SecondUser { get; set; }
    public Job? Job { get; set; }
    public bool IsBlocked { get; set; }
    public int? BlockedByUserId { get; set; }
    public User? BlockedByUser { get; set; }
    public DateTime? DeletedAtByUser { get; set; }
    public DateTime? DeletedAtBySecondParty { get; set; }
    public string LastMessageSnippet { get; set; } = string.Empty;
    public string LastMessageTime { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
}
