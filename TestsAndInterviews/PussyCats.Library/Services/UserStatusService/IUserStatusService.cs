using PussyCats.Library.DTOs;

namespace PussyCats.Library.Services.UserStatusService;

public interface IUserStatusService
{
    Task<IReadOnlyList<ApplicationCardModel>> GetApplicationsForUserAsync(int userId, CancellationToken cancellationToken = default);
}
