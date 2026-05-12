using PussyCats.Library.Domain.Enums;

namespace PussyCats.Library.Domain;

public class DeveloperInteraction
{
    public int DeveloperInteractionId { get; set; }
    public int DeveloperId { get; set; }
    public Developer Developer { get; set; }
    public int DeveloperPostId { get; set; }
    public DeveloperPost DeveloperPost { get; set; }
    public DeveloperInteractionType Type { get; set; }
}
