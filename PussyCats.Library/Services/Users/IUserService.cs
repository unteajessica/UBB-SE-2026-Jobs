using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.Users;

public interface IUserService
{
    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task RemoveAsync(int userId, CancellationToken cancellationToken = default);

    Task SetActiveAsync(int userId, bool isActive, CancellationToken cancellationToken = default);

    Task SetProfilePicturePathAsync(int userId, string profilePicturePath, CancellationToken cancellationToken = default);
}
