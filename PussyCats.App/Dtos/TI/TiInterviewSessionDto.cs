namespace PussyCats.App.Dtos.TI;

public class TiInterviewSessionDto
{
    public int Id { get; set; }
    public int PositionId { get; set; }
    public int? ExternalUserId { get; set; }
    public int InterviewerId { get; set; }
    public DateTime DateStart { get; set; }
    public string? Video { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Score { get; set; }
}
