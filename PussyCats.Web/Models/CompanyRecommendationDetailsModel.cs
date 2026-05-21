using PussyCats.Library.DTOs;

namespace PussyCats.Web.Models;

public class CompanyRecommendationDetailsModel
{
    public required UserApplicationResult Applicant { get; set; }

    public CompatibilityBreakdown? Breakdown { get; set; }
}
