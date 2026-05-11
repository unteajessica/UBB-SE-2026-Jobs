using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class WorkExperience
{
    public int WorkExperienceId { get; set; }

    //public int UserId { get; set; }
    [JsonIgnore] public User User { get; set; } = null!;

    public string Company { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool CurrentlyWorking { get; set; }
}
