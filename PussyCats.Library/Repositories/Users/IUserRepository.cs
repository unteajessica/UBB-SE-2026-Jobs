using PussyCats.Library.Domain;

namespace PussyCats.Library.Repositories.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task RemoveAsync(int userId, CancellationToken cancellationToken = default);

    Task UpdateActiveAccountAsync(int userId, bool isActive, CancellationToken cancellationToken = default);

    Task UpdateProfilePicturePathAsync(int userId, string profilePicturePath, CancellationToken cancellationToken = default);

    Task TouchLastUpdatedAsync(int userId, CancellationToken cancellationToken = default);
}
