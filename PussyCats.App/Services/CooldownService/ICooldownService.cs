namespace PussyCats_App.Services.CooldownService;

public interface ICooldownService
{
    Task<bool> IsOnCooldownAsync(int userId, int jobId, DateTime utcNow, CancellationToken cancellationToken = default);
}
