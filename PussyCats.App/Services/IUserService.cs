using PussyCats.Library.Domain;

namespace PussyCats.App.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(int userId, CancellationToken ct = default);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);

    Task<User> AddAsync(User user, CancellationToken ct = default);

    Task UpdateAsync(User user, CancellationToken ct = default);

    Task RemoveAsync(int userId, CancellationToken ct = default);
}
