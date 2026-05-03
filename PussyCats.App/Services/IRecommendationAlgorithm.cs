using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;

namespace PussyCats.App.Services;

/// <summary>
/// Computes job-applicant compatibility for the recommendation services.
/// Phase 3b ports the matchmaking RecommendationAlgorithm implementation
/// against this interface. Until then, no class implements it; DI must
/// not be wired in Phase 5 until the implementation lands.
/// </summary>
public interface IRecommendationAlgorithm
{
    double CalculateCompatibilityScore(
        User user,
        Job job,
        IReadOnlyList<UserSkill> userSkills,
        IReadOnlyList<JobSkill> jobSkills);

    CompatibilityBreakdown CalculateScoreBreakdown(
        User user,
        Job job,
        IReadOnlyList<UserSkill> userSkills,
        IReadOnlyList<JobSkill> jobSkills);
}
