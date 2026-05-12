using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Tests.Helpers;

public class MatchBuilder
{
    private int matchId = 1;
    private int userId = 1;
    private int jobId = 1;
    private MatchStatus status = MatchStatus.Applied;
    private DateTime timestamp = DateTime.UtcNow;
    private string feedback = string.Empty;

    public MatchBuilder WithId(int id)
    {
        matchId = id;
        return this;
    }

    public MatchBuilder AppliedFor(int user, int job)
    {
        userId = user;
        jobId = job;
        return this;
    }

    public MatchBuilder WithStatus(MatchStatus value)
    {
        status = value;
        return this;
    }

    public MatchBuilder WithFeedback(string value)
    {
        feedback = value;
        return this;
    }

    public MatchBuilder WithTimestamp(DateTime value)
    {
        timestamp = value;
        return this;
    }

    public Match Build() => new()
    {
        MatchId = matchId,
        User = new User { UserId = userId },
        JobId = jobId,
        Status = status,
        Timestamp = timestamp,
        FeedbackMessage = feedback,
    };
}
