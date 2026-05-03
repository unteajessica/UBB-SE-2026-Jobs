using PussyCats.Library.DTOs;

namespace PussyCats.App.Services;

public interface ICompanyRecommendationService
{
    bool HasMore { get; }

    Task LoadApplicantsAsync(int companyId, CancellationToken ct = default);

    UserApplicationResult? GetNextApplicant();

    void MoveToNext();

    void MoveToPrevious();

    Task<CompatibilityBreakdown?> GetBreakdownAsync(UserApplicationResult applicant, CancellationToken ct = default);
}
