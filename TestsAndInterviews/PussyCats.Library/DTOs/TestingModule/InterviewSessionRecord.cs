namespace PussyCats.Library.DTOs.TestingModule;

public class InterviewSessionRecord
{
    public int SessionId { get; set; }
    public int PositionId { get; set; }
    public int ExternalUserId { get; set; }
    public int InterviewerId { get; set; }
    public DateTime DateStart { get; set; }
    public string Video { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Score { get; set; }
}
