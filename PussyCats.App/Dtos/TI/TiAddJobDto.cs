namespace PussyCats.App.Dtos.TI;

public class TiAddJobDto
{
    public TiJobPostingDto JobPosting { get; set; } = new();
    public int CompanyId { get; set; }
    public List<TiJobSkillDto> SkillLinks { get; set; } = new();
}
