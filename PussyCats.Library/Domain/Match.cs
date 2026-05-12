using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public class Match
{
    public int MatchId { get; set; }

    public User User { get; set; } = null!;

    public Job Job { get; set; } = null!;

    public MatchStatus Status { get; set; } = MatchStatus.Applied;
    public DateTime Timestamp { get; set; }
    public string FeedbackMessage { get; set; } = string.Empty;
}
