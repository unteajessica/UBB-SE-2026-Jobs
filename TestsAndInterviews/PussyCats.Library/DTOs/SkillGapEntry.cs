namespace PussyCats.Library.DTOs;

public class SkillGapEntry
{
    public string SkillName { get; set; } = string.Empty;
    public int UserScore { get; set; }
    public int RequiredScore { get; set; }
    public int JobCount { get; set; }
}
