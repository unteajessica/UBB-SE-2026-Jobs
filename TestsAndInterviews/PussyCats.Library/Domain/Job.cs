using PussyCats.Library.Domain.Enums;
using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class Job
{
    public int JobId { get; set; }
    public int CompanyId { get; set; }

    public string? Photo { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string? IndustryField { get; set; }
    public string? JobType { get; set; }
    public string? ExperienceLevel { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? JobDescription { get; set; }
    public string? JobLocation { get; set; }
    public int AvailablePositions { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? Salary { get; set; }
    public int? AmountPayed { get; set; }
    public DateTime? Deadline { get; set; }

    public string? Location { get; set; }
    public string? EmploymentType { get; set; }

    public int? PromotionLevel { get; set; }
    [JsonIgnore]
    public JobRole? JobRole { get; set; }

    public Company Company { get; set; } = null!;

    public List<JobSkill> RequiredSkills { get; set; } = new();
    public List<Match> Matches { get; set; } = new();
}
