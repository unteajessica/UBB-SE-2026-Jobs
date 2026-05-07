using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public class DeveloperPost
{
    public int DeveloperPostId { get; set; }
    public int DeveloperId { get; set; }
    public DeveloperPostParameterType ParameterType { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
