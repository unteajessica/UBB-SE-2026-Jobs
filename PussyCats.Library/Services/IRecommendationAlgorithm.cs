using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;

namespace PussyCats.Library.Services;

/// <summary>
/// Computes job-applicant compatibility for the recommendation services.
/// </summary>
public interface IRecommendationAlgorithm
{
    /// <summary>
    /// Calculates the aggregate compatibility score for a candidate and a job.
    /// </summary>
    double CalculateCompatibilityScore(
        User user,
        Job job,
        IReadOnlyList<UserSkill> userSkills,
        IReadOnlyList<Domain.JobSkill> jobSkills);

    /// <summary>
    /// Calculates the score plus its individual component breakdown.
    /// </summary>
    CompatibilityBreakdown CalculateScoreBreakdown(
        User user,
        Job job,
        IReadOnlyList<UserSkill> userSkills,
        IReadOnlyList<Domain.JobSkill> jobSkills);
}
