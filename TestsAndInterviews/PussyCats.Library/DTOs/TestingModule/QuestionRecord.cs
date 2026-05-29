namespace PussyCats.Library.DTOs.TestingModule;

public class QuestionRecord
{
    public int QuestionId { get; set; }
    public int PositionId { get; set; }
    public int TestId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public decimal QuestionScore { get; set; }
    public string QuestionAnswer { get; set; } = string.Empty;
}
