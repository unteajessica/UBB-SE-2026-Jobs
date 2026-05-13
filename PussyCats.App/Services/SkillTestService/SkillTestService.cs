using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.SkillTests;

namespace PussyCats_App.Services.SkillTestService;

public class SkillTestService : ISkillTestService
{
    // Skill-test-specific rule, not a tier rule - stays on this class.
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
