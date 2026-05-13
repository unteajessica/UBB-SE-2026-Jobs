using PussyCats.Library.Domain;

namespace PussyCats_App.Services.SkillTestService;

public interface ISkillTestService
{
    Task<IReadOnlyList<SkillTest>> GetTestsForUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> CanRetakeTestAsync(int skillTestId, CancellationToken cancellationToken = default);

    Task<Badge> SubmitRetakeAsync(int skillTestId, int newScore, CancellationToken cancellationToken = default);

    Task<SkillTest?> GetSkillTestByIdAsync(int skillTestId, CancellationToken cancellationToken = default);
}
