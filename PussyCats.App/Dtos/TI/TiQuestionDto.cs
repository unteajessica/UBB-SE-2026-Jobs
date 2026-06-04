namespace PussyCats.App.Dtos.TI;

public class TiQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public float QuestionScore { get; set; }
    public string? QuestionAnswer { get; set; }
    public string? OptionsJson { get; set; }
    public int? TestId { get; set; }
}
