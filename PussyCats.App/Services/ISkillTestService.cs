using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface ISkillTestService
{
    Task<IReadOnlyList<SkillTest>> GetTestsForUserAsync(int userId, CancellationToken ct = default);

    Task<bool> CanRetakeTestAsync(int skillTestId, CancellationToken ct = default);

    Task<Badge> SubmitRetakeAsync(int skillTestId, int newScore, CancellationToken ct = default);

    Task<SkillTest?> GetSkillTestByIdAsync(int skillTestId, CancellationToken ct = default);
}
