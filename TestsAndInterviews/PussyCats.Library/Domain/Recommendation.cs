namespace PussyCats.Library.Domain;

public class Recommendation
{
    public int RecommendationId { get; set; }

    public User User { get; set; } = null!;

    public Job Job { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
