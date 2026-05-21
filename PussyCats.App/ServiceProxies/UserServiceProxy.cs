using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.Users;

namespace PussyCats.App.ServiceProxies;

public class UserServiceProxy : IUserService
{
    private readonly HttpClient http;

    // API serialises enums as strings (JsonStringEnumConverter, registered globally).
    // User.PersonalityResult.SelectedRole is a nullable JobRole, so deserialising with
    // default options throws "could not be converted to System.Nullable`1[JobRole]".
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

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
        return await response.Content.ReadFromJsonAsync<User>(JsonOptions, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync("api/users", user, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<User>(JsonOptions, cancellationToken))!;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var response = await http.PutAsJsonAsync($"api/users/{user.UserId}", user, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveAsync(int userId, CancellationToken cancellationToken = default)
    {
        var response = await http.DeleteAsync($"api/users/{userId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetActiveAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        var response = await http.PatchAsJsonAsync($"api/users/{userId}/active", new { isActive }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetProfilePicturePathAsync(int userId, string profilePicturePath, CancellationToken cancellationToken = default)
    {
        var response = await http.PatchAsJsonAsync($"api/users/{userId}/profile-picture", new { path = profilePicturePath }, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
