namespace PussyCats.Library.DTOs.TestingModule;

public class TestAttemptRecord
{
    public int UserTestId { get; set; }
    public int TestId { get; set; }
    public int ExternalUserId { get; set; }
    public decimal Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string AnswersFilePath { get; set; } = string.Empty;
}
