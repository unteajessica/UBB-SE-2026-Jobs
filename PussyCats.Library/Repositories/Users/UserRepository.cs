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
        ApplyDetachedUpdate(user, user.UserId);
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
    private void ApplyDetachedUpdate(User incoming, int key)
    {
        var tracked = databaseContext.Users.Local.FirstOrDefault(existing => existing.UserId == key);
        if (tracked is not null)
        {
            databaseContext.Entry(tracked).CurrentValues.SetValues(incoming);
        }
        else
        {
            databaseContext.Entry(incoming).State = EntityState.Modified;
        }
    }

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
