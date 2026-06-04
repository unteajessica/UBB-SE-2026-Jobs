namespace PussyCats.App.Dtos.TI;

public class TiApplicantDto
{
    public int ApplicantId { get; set; }
    public int JobId { get; set; }
    public int UserId { get; set; }
    public decimal? AppTestGrade { get; set; }
    public decimal? CvGrade { get; set; }
    public decimal? CompanyTestGrade { get; set; }
    public decimal? InterviewGrade { get; set; }
    public string ApplicationStatus { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public int? RecommendedFromCompanyId { get; set; }
    public string? CvFileUrl { get; set; }
}
