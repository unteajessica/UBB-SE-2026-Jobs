using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.Tests.Fakes;

public class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<int, User> store = new();

    public void Seed(params User[] users)
    {
        foreach (var user in users)
        {
            store[user.UserId] = user;
        }
    }

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        store.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<User> snapshot = store.Values.ToList();
        return Task.FromResult(snapshot);
    }

    public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user.UserId == 0)
        {
            user.UserId = NextId();
        }
        var now = DateTime.UtcNow;
        if (user.CreatedAt == default)
        {
            user.CreatedAt = now;
        }
        user.LastUpdated = now;
        store[user.UserId] = user;
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.LastUpdated = DateTime.UtcNow;
        store[user.UserId] = user;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int userId, CancellationToken cancellationToken = default)
    {
        store.Remove(userId);
        return Task.CompletedTask;
    }

    public Task UpdateActiveAccountAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        if (store.TryGetValue(userId, out var user))
        {
            user.ActiveAccount = isActive;
            user.LastUpdated = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task UpdateProfilePicturePathAsync(int userId, string profilePicturePath, CancellationToken cancellationToken = default)
    {
        if (store.TryGetValue(userId, out var user))
        {
            user.ProfilePicturePath = profilePicturePath;
            user.LastUpdated = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task TouchLastUpdatedAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (store.TryGetValue(userId, out var user))
        {
            user.LastUpdated = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    private int NextId() => store.Count == 0 ? 1 : store.Keys.Max() + 1;
}
