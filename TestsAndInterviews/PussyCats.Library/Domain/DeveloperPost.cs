using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public class DeveloperPost
{
    public int DeveloperPostId { get; set; }
    public Developer Developer { get; set; } = null!;
    public DeveloperPostParameterType ParameterType { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
