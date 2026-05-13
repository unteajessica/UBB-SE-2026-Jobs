using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Skills;

namespace PussyCats.Tests.Fakes;

public class FakeUserSkillRepository : IUserSkillRepository
{
    private readonly Dictionary<(int UserId, int SkillId), UserSkill> userSkillsById = new();

    public void Seed(params UserSkill[] userSkills)
    {
        foreach (var userSkill in userSkills)
        {
            userSkillsById[(userSkill.User.UserId, userSkill.Skill.SkillId)] = userSkill;
        }
    }

    public Task<UserSkill?> GetAsync(int userId, int skillId, CancellationToken cancellationToken = default)
    {
        userSkillsById.TryGetValue((userId, skillId), out var userSkill);
        return Task.FromResult(userSkill);
    }

    public Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<UserSkill> filtered = userSkillsById.Values.Where(userSkill => userSkill.User.UserId == userId).ToList();
        return Task.FromResult(filtered);
    }

    public Task<IReadOnlyList<UserSkill>> GetVerifiedByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<UserSkill> filtered = userSkillsById.Values
            .Where(userSkill => userSkill.User.UserId == userId && userSkill.IsVerified && userSkill.AchievedDate != null)
            .ToList();
        return Task.FromResult(filtered);
    }

    public Task<UserSkill> AddAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
    {
        userSkillsById[(userSkill.User.UserId, userSkill.Skill.SkillId)] = userSkill;
        return Task.FromResult(userSkill);
    }

    public Task UpdateAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
    {
        userSkillsById[(userSkill.User.UserId, userSkill.Skill.SkillId)] = userSkill;
        return Task.CompletedTask;
    }

    public Task UpdateScoreAsync(int userId, int skillId, int score, CancellationToken cancellationToken = default)
    {
        if (userSkillsById.TryGetValue((userId, skillId), out var userSkill))
        {
            userSkill.Score = score;
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int userId, int skillId, CancellationToken cancellationToken = default)
    {
        userSkillsById.Remove((userId, skillId));
        return Task.CompletedTask;
    }
}
