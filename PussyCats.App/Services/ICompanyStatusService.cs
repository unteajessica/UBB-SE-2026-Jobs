using PussyCats.Library.DTOs;

namespace PussyCats.App.Services;

public interface ICompanyStatusService
{
    Task<UserApplicationResult?> GetApplicantByMatchIdAsync(int companyId, int matchId, CancellationToken ct = default);

    Task<IReadOnlyList<UserApplicationResult>> GetApplicantsForCompanyAsync(int companyId, CancellationToken ct = default);
}
