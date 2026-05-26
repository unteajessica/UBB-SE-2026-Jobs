using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Users;

namespace PussyCats.Library.ServiceProxies;

public class UserServiceProxy : IUserService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient http;

    public UserServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<User>>("api/users", JsonOptions, cancellationToken) ?? new List<User>();

    public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/users/{userId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions, cancellationToken: cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/users/by-email/{Uri.EscapeDataString(email)}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions, cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/users/exists-by-email/{Uri.EscapeDataString(email)}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/users", user, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<User>(JsonOptions, cancellationToken: cancellationToken))!;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/users/{user.UserId}", user, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(int userId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/users/{userId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetActiveAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        var response = await http.PatchAsJsonAsync($"api/users/{userId}/active", new { isActive }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetProfilePicturePathAsync(int userId, string profilePicturePath, CancellationToken cancellationToken = default)
    {
        var response = await http.PatchAsJsonAsync($"api/users/{userId}/profile-picture", new { path = profilePicturePath }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
