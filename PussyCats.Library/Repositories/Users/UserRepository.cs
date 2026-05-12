using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;

namespace PussyCats.Library.Repositories.Users;

public class UserRepository : IUserRepository
{
    private readonly PussyCatsDbContext databaseContext;

    public UserRepository(PussyCatsDbContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <summary>
    /// Includes WorkExperiences, Projects, ExtraCurricularActivities, Skills.Skill, and
    /// PersonalityResult.TraitScores so callers receive a fully populated profile in one round trip.
    /// Tracked because the typical caller (UserService.UpdateProfile) mutates the entity.
    /// </summary>
    public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await databaseContext.Users
            .Include(user => user.WorkExperiences)
            .Include(user => user.Projects)
            .Include(user => user.ExtraCurricularActivities)
            .Include(user => user.Skills).ThenInclude(skill => skill.Skill)
            .Include(user => user.PersonalityResult)!.ThenInclude(result => result!.TraitScores)
            .FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Read-only listing — no Includes to keep the query light. Callers that need detail load
    /// it through GetByIdAsync.
    /// </summary>
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        if (user.CreatedAt == default)
        {
            user.CreatedAt = now;
        }
        user.LastUpdated = now;

        databaseContext.Users.Add(user);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.LastUpdated = DateTime.UtcNow;
        var target = await GetTrackedOrLoadedUserAsync(user.UserId, cancellationToken).ConfigureAwait(false);
        if (target is null)
        {
            databaseContext.Entry(user).State = EntityState.Modified;
            await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        databaseContext.Entry(target).CurrentValues.SetValues(user);
        await ReconcileProfileCollectionsAsync(target, user, cancellationToken).ConfigureAwait(false);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Reconciles a detached request-body entity with whatever the DbContext is
    /// already tracking. Controllers typically call <c>GetByIdAsync</c> as a 404
    /// guard before <c>UpdateAsync</c>, which leaves the existing User tracked;
    /// passing a fresh deserialised User to <c>Update</c> or
    /// <c>Entry().State = Modified</c> then throws "another instance with the
    /// same key value is already being tracked." Copying the request values onto
    /// the tracked instance avoids the IdentityMap conflict.
    /// </summary>
    private async Task<User?> GetTrackedOrLoadedUserAsync(int key, CancellationToken cancellationToken)
    {
        var tracked = databaseContext.Users.Local.FirstOrDefault(existing => existing.UserId == key);
        if (tracked is not null)
        {
            return tracked;
        }

        return await databaseContext.Users
            .Include(user => user.WorkExperiences)
            .Include(user => user.Projects)
            .Include(user => user.ExtraCurricularActivities)
            .Include(user => user.Skills).ThenInclude(skill => skill.Skill)
            .FirstOrDefaultAsync(user => user.UserId == key, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ReconcileProfileCollectionsAsync(User target, User incoming, CancellationToken cancellationToken)
    {
        ReplaceWorkExperiences(target, incoming.WorkExperiences);
        ReplaceProjects(target, incoming.Projects);
        ReplaceExtraCurricularActivities(target, incoming.ExtraCurricularActivities);
        await ReconcileSkillsAsync(target, incoming.Skills, cancellationToken).ConfigureAwait(false);
    }

    private void ReplaceWorkExperiences(User target, IEnumerable<WorkExperience> incoming)
    {
        databaseContext.Set<WorkExperience>().RemoveRange(target.WorkExperiences);
        target.WorkExperiences.Clear();

        foreach (var workExperience in incoming)
        {
            target.WorkExperiences.Add(new WorkExperience
            {
                User = target,
                Company = workExperience.Company,
                JobTitle = workExperience.JobTitle,
                StartDate = workExperience.StartDate,
                EndDate = workExperience.EndDate,
                Description = workExperience.Description,
                CurrentlyWorking = workExperience.CurrentlyWorking,
            });
        }
    }

    private void ReplaceProjects(User target, IEnumerable<Project> incoming)
    {
        databaseContext.Set<Project>().RemoveRange(target.Projects);
        target.Projects.Clear();

        foreach (var project in incoming)
        {
            target.Projects.Add(new Project
            {
                UserId = target.UserId,
                Name = project.Name,
                Description = project.Description,
                Url = project.Url,
                Technologies = project.Technologies.ToList(),
            });
        }
    }

    private void ReplaceExtraCurricularActivities(User target, IEnumerable<ExtraCurricularActivity> incoming)
    {
        databaseContext.Set<ExtraCurricularActivity>().RemoveRange(target.ExtraCurricularActivities);
        target.ExtraCurricularActivities.Clear();

        foreach (var activity in incoming)
        {
            target.ExtraCurricularActivities.Add(new ExtraCurricularActivity
            {
                User = target,
                ActivityName = activity.ActivityName,
                Organization = activity.Organization,
                Role = activity.Role,
                Period = activity.Period,
                Description = activity.Description,
            });
        }
    }

    private async Task ReconcileSkillsAsync(User target, IEnumerable<UserSkill> incoming, CancellationToken cancellationToken)
    {
        var desiredSkills = incoming
            .Select(CreateSkillDraft)
            .Where(skill => skill is not null)
            .Select(skill => skill!)
            .GroupBy(skill => skill.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        var desiredNames = desiredSkills.Select(skill => skill.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var removedSkills = target.Skills
            .Where(skill => !desiredNames.Contains(skill.Skill?.Name ?? string.Empty))
            .ToList();

        foreach (var removedSkill in removedSkills)
        {
            databaseContext.UserSkills.Remove(removedSkill);
            target.Skills.Remove(removedSkill);
        }

        foreach (var desiredSkill in desiredSkills)
        {
            var catalogSkill = await ResolveCatalogSkillAsync(desiredSkill.Name, cancellationToken).ConfigureAwait(false);
            var existing = target.Skills.FirstOrDefault(skill =>
                string.Equals(skill.Skill?.Name, catalogSkill.Name, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.Skill = catalogSkill;
               // existing.SkillId = catalogSkill.SkillId;
                if (!existing.IsVerified)
                {
                    existing.Score = desiredSkill.Score;
                    existing.IsVerified = desiredSkill.IsVerified;
                    existing.AchievedDate = desiredSkill.AchievedDate;
                }
                continue;
            }

            target.Skills.Add(new UserSkill
            {
                User = target,
                Skill = catalogSkill,
                //SkillId = catalogSkill.SkillId,
                Score = desiredSkill.Score,
                IsVerified = desiredSkill.IsVerified,
                AchievedDate = desiredSkill.AchievedDate,
            });
        }
    }

    private async Task<Skill> ResolveCatalogSkillAsync(string name, CancellationToken cancellationToken)
    {
        var local = databaseContext.Skills.Local.FirstOrDefault(skill =>
            string.Equals(skill.Name, name, StringComparison.OrdinalIgnoreCase));
        if (local is not null)
        {
            return local;
        }

        var existing = await databaseContext.Skills
            .FirstOrDefaultAsync(skill => skill.Name == name, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            return existing;
        }

        var added = new Skill { Name = name, Category = "Custom" };
        databaseContext.Skills.Add(added);
        return added;
    }

    private static SkillDraft? CreateSkillDraft(UserSkill userSkill)
    {
        var name = userSkill.Skill?.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return new SkillDraft(name, userSkill.Score, userSkill.IsVerified, userSkill.AchievedDate);
    }

    private sealed record SkillDraft(string Name, int Score, bool IsVerified, DateOnly? AchievedDate);

    public async Task RemoveAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await databaseContext.Users.FindAsync(new object?[] { userId }, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return;
        }
        databaseContext.Users.Remove(user);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Soft-delete primitive — toggles ActiveAccount without touching match history. Original
    /// implementation: PussyCatsApp UserProfileRepository.UpdateAccountStatus, which mapped a
    /// "ACTIVE" string to a boolean; that string indirection is dropped, callers pass the bool
    /// directly.
    /// </summary>
    public async Task UpdateActiveAccountAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await databaseContext.Users.FindAsync(new object?[] { userId }, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return;
        }
        user.ActiveAccount = isActive;
        user.LastUpdated = DateTime.UtcNow;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp UserProfileRepository.UpdateProfilePicture. The original handled
    /// null by writing DBNull; here the column is non-nullable string, so callers pass an empty
    /// string when they intend "no picture".
    /// </summary>
    public async Task UpdateProfilePicturePathAsync(int userId, string profilePicturePath, CancellationToken cancellationToken = default)
    {
        var user = await databaseContext.Users.FindAsync(new object?[] { userId }, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return;
        }
        user.ProfilePicturePath = profilePicturePath;
        user.LastUpdated = DateTime.UtcNow;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Original: PussyCatsApp UserProfileRepository.UpdateProfileLastModified. The original took
    /// a caller-supplied timestamp; the new contract uses server time so the audit trail is
    /// authoritative.
    /// </summary>
    public async Task TouchLastUpdatedAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await databaseContext.Users.FindAsync(new object?[] { userId }, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return;
        }
        user.LastUpdated = DateTime.UtcNow;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
