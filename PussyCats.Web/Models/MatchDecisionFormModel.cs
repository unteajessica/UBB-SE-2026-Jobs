using System.ComponentModel.DataAnnotations;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Web.Models;

public class MatchDecisionFormModel
{
    public const int MaximumFeedbackLength = 500;

    [Required]
    [Range(1, int.MaxValue)]
    public int MatchId { get; set; }

    [Required(ErrorMessage = "Select a decision.")]
    [Display(Name = "Decision")]
    public MatchStatus? Decision { get; set; }

    [Required(ErrorMessage = "Feedback is required.")]
    [StringLength(MaximumFeedbackLength, ErrorMessage = "Feedback must be 500 characters or fewer.")]
    [Display(Name = "Feedback")]
    public string Feedback { get; set; } = string.Empty;

    public string ApplicantName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public MatchStatus CurrentStatus { get; set; }

    public DateTime Timestamp { get; set; }
}
