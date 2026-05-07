using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.SkillTests;

namespace PussyCats.App.Services;

internal static class SkillTestDefaults
{
    public static async Task<IReadOnlyList<SkillTest>> GetOrCreateAsync(
        ISkillTestRepository repository,
        int userId,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (existing.Count > 0)
        {
            return existing;
        }

        var achievedDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4));
        var defaults = new[]
        {
            new SkillTest { UserId = userId, Name = "C# Fundamentals", Score = 82, AchievedDate = achievedDate },
            new SkillTest { UserId = userId, Name = "SQL Server", Score = 76, AchievedDate = achievedDate },
            new SkillTest { UserId = userId, Name = "Software Design", Score = 88, AchievedDate = achievedDate },
        };

        var created = new List<SkillTest>(defaults.Length);
        foreach (var test in defaults)
        {
            created.Add(await repository.AddAsync(test, cancellationToken).ConfigureAwait(false));
        }

        return created;
    }
}
