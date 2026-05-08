using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Tests.Fakes;

public class FakeUserSkillRepository : IUserSkillRepository
{
    private readonly Dictionary<(int UserId, int SkillId), UserSkill> store = new();

    public void Seed(params UserSkill[] userSkills)
    {
        foreach (var userSkill in userSkills)
        {
            store[(userSkill.UserId, userSkill.SkillId)] = userSkill;
        }
    }

    public Task<UserSkill?> GetAsync(int userId, int skillId, CancellationToken cancellationToken = default)
    {
        store.TryGetValue((userId, skillId), out var userSkill);
        return Task.FromResult(userSkill);
    }

    public Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<UserSkill> filtered = store.Values.Where(userSkill => userSkill.UserId == userId).ToList();
        return Task.FromResult(filtered);
    }

    public Task<IReadOnlyList<UserSkill>> GetVerifiedByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<UserSkill> filtered = store.Values
            .Where(userSkill => userSkill.UserId == userId && userSkill.IsVerified && userSkill.AchievedDate != null)
            .ToList();
        return Task.FromResult(filtered);
    }

    public Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
    {
        store[(userSkill.UserId, userSkill.SkillId)] = userSkill;
        return Task.FromResult(userSkill);
    }

    public Task UpdateAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
    {
        store[(userSkill.UserId, userSkill.SkillId)] = userSkill;
        return Task.CompletedTask;
    }

    public Task UpdateScoreAsync(int userId, int skillId, int score, CancellationToken cancellationToken = default)
    {
        if (store.TryGetValue((userId, skillId), out var userSkill))
        {
            userSkill.Score = score;
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int userId, int skillId, CancellationToken cancellationToken = default)
    {
        store.Remove((userId, skillId));
        return Task.CompletedTask;
    }
}
