namespace PussyCats.Library.DTOs;

public class MatchStatistics
{
    public int TotalMatches { get; set; }
    public Dictionary<string, int> MatchesPerPosition { get; set; } = new();
    public int MatchesLastMonth { get; set; }
    public int MatchesLastSixMonths { get; set; }
    public int MatchesLastYear { get; set; }
}
