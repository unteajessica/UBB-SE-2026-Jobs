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

    public async Task<User?> GetByIdAsync(int userId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<User>(http, $"api/users/{userId}", ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetListAsync<User>(http, "api/users", ct).ConfigureAwait(false);
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("api/users", user, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<User>(response, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        using var response = await http.PutAsJsonAsync($"api/users/{user.UserId}", user, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int userId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/users/{userId}", ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task UpdateActiveAccountAsync(int userId, bool isActive, CancellationToken ct = default)
    {
        using var response = await http.PatchAsJsonAsync($"api/users/{userId}/active", new { isActive }, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task UpdateProfilePicturePathAsync(int userId, string profilePicturePath, CancellationToken ct = default)
    {
        using var response = await http.PatchAsJsonAsync(
            $"api/users/{userId}/profile-picture",
            new { path = profilePicturePath },
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public Task TouchLastUpdatedAsync(int userId, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
