using PussyCats.Library.DTOs;

namespace PussyCats_App.Services.CompanyStatusService;

public interface ICompanyStatusService
{
    Task<UserApplicationResult?> GetApplicantByMatchIdAsync(int companyId, int matchId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserApplicationResult>> GetApplicantsForCompanyAsync(int companyId, CancellationToken cancellationToken = default);
}
