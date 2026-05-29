using PussyCats.Library.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.SkillTests;

namespace PussyCats.Library.Services.SkillTests;

public class SkillTestService : ISkillTestService
{
    private const int RetakeEligibilityMonths = 3;

    private readonly ISkillTestRepository skillTestRepository;

    public SkillTestService(ISkillTestRepository skillTestRepository)
    {
        this.skillTestRepository = skillTestRepository;
    }

    public async Task<IReadOnlyList<SkillTest>> GetTestsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await skillTestRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> CanRetakeTestAsync(int skillTestId, CancellationToken cancellationToken = default)
    {
        var skillTest = await skillTestRepository.GetByIdAsync(skillTestId, cancellationToken).ConfigureAwait(false);

        if (skillTest is null)
        {
            throw new Exception($"No test found for ID {skillTestId}");
        }

        return IsRetakeEligible(skillTest);
    }

    public async Task<Badge> SubmitRetakeAsync(int skillTestId, int newScore, CancellationToken cancellationToken = default)
    {
        if (!await CanRetakeTestAsync(skillTestId, cancellationToken).ConfigureAwait(false))
        {
            throw new Exception("Test is not yet eligible for a retake. Action blocked at service layer.");
        }

        await skillTestRepository.UpdateScoreAsync(skillTestId, newScore, cancellationToken).ConfigureAwait(false);
        await skillTestRepository.UpdateAchievedDateAsync(skillTestId, DateOnly.FromDateTime(DateTime.Now), cancellationToken).ConfigureAwait(false);

        return SimpleModelOperations.AssignTier(newScore);
    }

    public async Task<SkillTest> AddSkillTestAsync(SkillTest skillTest, CancellationToken cancellationToken = default)
    {
        SkillTest resultSkillTest = await skillTestRepository.AddAsync(skillTest, cancellationToken).ConfigureAwait(false);
        return resultSkillTest;
    }

    public async Task UpdateScoreAsync(int skillTestId, int newScore, CancellationToken cancellationToken = default)
    {
        await skillTestRepository.UpdateScoreAsync(skillTestId, newScore, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAchievedDateAsync(int skillTestId, DateOnly newDate, CancellationToken cancellationToken = default)
    {
        await skillTestRepository.UpdateAchievedDateAsync(skillTestId, newDate, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int skillTestId, CancellationToken cancellationToken = default)
    {
        await skillTestRepository.RemoveAsync(skillTestId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SkillTest?> GetSkillTestByIdAsync(int skillTestId, CancellationToken cancellationToken = default)
    {
        return await skillTestRepository.GetByIdAsync(skillTestId, cancellationToken).ConfigureAwait(false);
    }

    public static bool IsRetakeEligible(SkillTest skillTest)
    {
        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
        DateOnly eligibilityDate = currentDate.AddMonths(-RetakeEligibilityMonths);

        return eligibilityDate >= skillTest.AchievedDate;
    }

    public static string AchievedDateFormatted(SkillTest skillTest)
    {
        return skillTest.AchievedDate.ToString("dd.MM.yyyy");
    }
}
