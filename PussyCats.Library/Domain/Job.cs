using PussyCats.Library.Domain.Enums;
using System.Text.Json.Serialization;

namespace PussyCats.Library.Domain;

public class Job
{
    public int JobId { get; set; }

    public string JobTitle { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public int PromotionLevel { get; set; }
    public JobRole JobRole { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public List<JobSkill> RequiredSkills { get; set; } = new();
    public List<Match> Matches { get; set; } = new();
}
