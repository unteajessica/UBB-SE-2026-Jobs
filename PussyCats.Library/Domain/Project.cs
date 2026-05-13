using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class Project
{
    public int ProjectId { get; set; }

    [JsonIgnore] public User User { get; set; } = null!;//TODO: check if JsonIgnore is needed or works.

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Technologies { get; set; } = new();
    public string Url { get; set; } = string.Empty;
}
