namespace PussyCats.App.Dtos.TI;

public class TiJobPostingDto
{
    public int JobId { get; set; }
    public int CompanyId { get; set; }
    public string? Photo { get; set; }
    public string? JobTitle { get; set; }
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
    public List<TiJobSkillDto> JobSkills { get; set; } = new();
}
