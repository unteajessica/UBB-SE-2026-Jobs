using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.SkillTests;

namespace PussyCats.Tests.Fakes;

public class FakeSkillTestRepository : ISkillTestRepository
{
    private readonly Dictionary<int, SkillTest> skillTestsById = new();

    public void Seed(params SkillTest[] skillTests)
    {
        foreach (var skillTest in skillTests)
        {
            this.skillTestsById[skillTest.SkillTestId] = skillTest;
        }
    }

    public Task<SkillTest?> GetByIdAsync(int skillTestId, CancellationToken cancellationToken = default)
    {
        skillTestsById.TryGetValue(skillTestId, out var skillTest);
        return Task.FromResult(skillTest);
    }

    public Task<IReadOnlyList<SkillTest>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SkillTest> filtered = skillTestsById.Values.Where(skillTest => skillTest.User.UserId == userId).ToList();
        return Task.FromResult(filtered);
    }

    public Task<SkillTest> AddAsync(SkillTest skillTest, CancellationToken cancellationToken = default)
    {
        if (skillTest.SkillTestId == 0)
        {
            skillTest.SkillTestId = NextId();
        }
        skillTestsById[skillTest.SkillTestId] = skillTest;
        return Task.FromResult(skillTest);
    }

    public Task UpdateScoreAsync(int skillTestId, int score, CancellationToken cancellationToken = default)
    {
        if (skillTestsById.TryGetValue(skillTestId, out var skillTest))
        {
            skillTest.Score = score;
        }
        return Task.CompletedTask;
    }

    public Task UpdateAchievedDateAsync(int skillTestId, DateOnly achievedDate, CancellationToken cancellationToken = default)
    {
        if (skillTestsById.TryGetValue(skillTestId, out var skillTest))
        {
            skillTest.AchievedDate = achievedDate;
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int skillTestId, CancellationToken cancellationToken = default)
    {
        skillTestsById.Remove(skillTestId);
        return Task.CompletedTask;
    }

    private int NextId() => skillTestsById.Count == 0 ? 1 : skillTestsById.Keys.Max() + 1;
}
