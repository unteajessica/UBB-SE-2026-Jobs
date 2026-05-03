using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.App.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;

    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken ct = default)
    {
        return await userRepository.GetByIdAsync(userId, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await userRepository.GetAllAsync(ct).ConfigureAwait(false);
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        return await userRepository.AddAsync(user, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        await userRepository.UpdateAsync(user, ct).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int userId, CancellationToken ct = default)
    {
        await userRepository.RemoveAsync(userId, ct).ConfigureAwait(false);
    }
}
