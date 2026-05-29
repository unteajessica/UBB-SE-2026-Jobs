using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.DTOs;

public class RoleResult
{
    public JobRole JobRole { get; set; }
    public double MatchScore { get; set; }
    public List<Suggestion> Suggestions { get; set; } = new();
}
