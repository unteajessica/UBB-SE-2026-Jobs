using PussyCats.Library.DTOs;

namespace PussyCats.App.Services;

public interface IUserStatusService
{
    Task<IReadOnlyList<ApplicationCardModel>> GetApplicationsForUserAsync(int userId, CancellationToken cancellationToken = default);
}
