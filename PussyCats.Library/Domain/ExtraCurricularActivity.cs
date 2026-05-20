using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class ExtraCurricularActivity
{
    public int ExtraCurricularActivityId { get; set; }

    [JsonIgnore] public User User { get; set; } = null!;

    public string ActivityName { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
