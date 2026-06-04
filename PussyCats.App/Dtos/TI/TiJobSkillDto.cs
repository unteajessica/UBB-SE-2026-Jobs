namespace PussyCats.App.Dtos.TI;

public class TiJobSkillDto
{
    public int SkillId { get; set; }
    public int JobId { get; set; }
    public int RequiredPercentage { get; set; }
    public TiSkillDto? SkillDto { get; set; }
}
