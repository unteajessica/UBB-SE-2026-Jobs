namespace PussyCats.App.Dtos.TI;

public class TiSlotDto
{
    public int Id { get; set; }
    public int RecruiterId { get; set; }
    public int? CandidateId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Duration { get; set; }
    public int Status { get; set; }
    public string InterviewType { get; set; } = string.Empty;
    public string TimeRange => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
}
