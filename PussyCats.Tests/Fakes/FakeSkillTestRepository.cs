using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.SkillTests;

namespace PussyCats.Tests.Fakes;

public class FakeSkillTestRepository : ISkillTestRepository
{
    private readonly Dictionary<int, SkillTest> store = new();

    public void Seed(params SkillTest[] skillTests)
    {
        foreach (var skillTest in skillTests)
        {
            store[skillTest.SkillTestId] = skillTest;
        }
    }

    public Task<SkillTest?> GetByIdAsync(int skillTestId, CancellationToken cancellationToken = default)
    {
        store.TryGetValue(skillTestId, out var skillTest);
        return Task.FromResult(skillTest);
    }

    public Task<IReadOnlyList<SkillTest>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SkillTest> filtered = store.Values.Where(skillTest => skillTest.UserId == userId).ToList();
        return Task.FromResult(filtered);
    }

    public Task<SkillTest> AddAsync(SkillTest skillTest, CancellationToken cancellationToken = default)
    {
        if (skillTest.SkillTestId == 0)
        {
            skillTest.SkillTestId = NextId();
        }
        store[skillTest.SkillTestId] = skillTest;
        return Task.FromResult(skillTest);
    }

    public Task UpdateScoreAsync(int skillTestId, int score, CancellationToken cancellationToken = default)
    {
        if (store.TryGetValue(skillTestId, out var skillTest))
        {
            skillTest.Score = score;
        }
        return Task.CompletedTask;
    }

    public Task UpdateAchievedDateAsync(int skillTestId, DateOnly achievedDate, CancellationToken cancellationToken = default)
    {
        if (store.TryGetValue(skillTestId, out var skillTest))
        {
            skillTest.AchievedDate = achievedDate;
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int skillTestId, CancellationToken cancellationToken = default)
    {
        store.Remove(skillTestId);
        return Task.CompletedTask;
    }

    private int NextId() => store.Count == 0 ? 1 : store.Keys.Max() + 1;
}
