using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.Users;

namespace PussyCats.App.RepositoryProxies;

public class UserRepositoryProxy : IUserRepository
{
    private readonly HttpClient http;

    public UserRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<User>(http, $"api/users/{userId}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<User>(
            http,
            $"api/users/by-email/{Uri.EscapeDataString(email)}",
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<bool>(
            $"api/users/exists-by-email/{Uri.EscapeDataString(email)}",
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetListAsync<User>(http, "api/users", cancellationToken).ConfigureAwait(false);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/users", user, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<User>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"api/users/{user.UserId}", user, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/users/{userId}", cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task UpdateActiveAccountAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        using var response = await http.PatchAsJsonAsync($"api/users/{userId}/active", new { isActive }, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task UpdateProfilePicturePathAsync(int userId, string profilePicturePath, CancellationToken cancellationToken = default)
    {
        using var response = await http.PatchAsJsonAsync(
            $"api/users/{userId}/profile-picture",
            new { path = profilePicturePath },
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public Task TouchLastUpdatedAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
