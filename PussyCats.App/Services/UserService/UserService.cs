using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Users;

namespace PussyCats_App.Services.UserService;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;

    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await userRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return await userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        await userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int userId, CancellationToken cancellationToken = default)
    {
        await userRepository.RemoveAsync(userId, cancellationToken).ConfigureAwait(false);
    }
}
