using PussyCats.Library.DTOs;

namespace PussyCats.Library.Services.CompanyRecommendationService;

public interface ICompanyRecommendationService
{
    bool HasMore { get; }

    Task<IReadOnlyList<UserApplicationResult>> GetRankedApplicantsAsync(int companyId, CancellationToken cancellationToken = default);

    Task<UserApplicationResult?> GetApplicantByMatchIdAsync(int companyId, int matchId, CancellationToken cancellationToken = default);

    Task LoadApplicantsAsync(int companyId, CancellationToken cancellationToken = default);

    UserApplicationResult? GetNextApplicant();

    void MoveToNext();

    void MoveToPrevious();

    Task<CompatibilityBreakdown?> GetBreakdownAsync(UserApplicationResult applicant, CancellationToken cancellationToken = default);
}
