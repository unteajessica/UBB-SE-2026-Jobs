using System.ComponentModel.DataAnnotations;

namespace PussyCats.Web.Models;

// Bound by the Create/Edit views. The domain Recommendation has User and Job
// navigation properties which can't be edited as nested forms — this model
// captures the foreign-key ids and the timestamp, which is everything the
// service needs.
public class RecommendationFormModel
{
    public int RecommendationId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "User is required.")]
    [Display(Name = "User")]
    public int UserId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Job is required.")]
    [Display(Name = "Job")]
    public int JobId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
