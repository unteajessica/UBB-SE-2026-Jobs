using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.UserProfileService;


namespace PussyCats.Library.ServiceProxies
{
    public class UserProfileServiceProxy : IUserProfileService
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public UserProfileServiceProxy(HttpClient http)
        {
            _http = http;
        }

        public async Task<User?> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _http.GetFromJsonAsync<User>($"api/users/{userId}", _jsonOptions, cancellationToken);
        }

        public async Task<int> RecalculateLevelAsync(User user, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"api/users/{user.UserId}/experience", cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<XpResponse>(_jsonOptions, cancellationToken);
            return data?.TotalExperiencePoints ?? 0;
        }

        public async Task UpdateAccountStatusAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
        {
            var response = await _http.PatchAsync($"api/users/{userId}/active", JsonContent.Create(new { IsActive = isActive }), cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task<IReadOnlyList<SkillTest>> GetSkillTestsForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            /*
           var response = await _http.GetAsync($"api/users/{userId}/skill-tests", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<SkillTest>>(_jsonOptions, cancellationToken) ?? new List<SkillTest>(); 
            */

            var response = await _http.GetAsync($"api/skill-tests?userId={userId}", cancellationToken); 
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<SkillTest>>(_jsonOptions, cancellationToken) ?? new List<SkillTest>();
        }

        public async Task<bool> IsProfileAvailableAsync(int userId, CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync($"api/users/{userId}/is-active", cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<bool?>(_jsonOptions, cancellationToken);
            return data ?? false;
        }

        // This is techincally not really correct because SaveAsync should create a new user if the userId doesn't exist, but I think it should work.
        public async Task SaveAsync(int userId, User user, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"api/users/{userId}/profile", user, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task<User> UploadCvAsync(int userId, Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "file", fileName);
            var response = await _http.PostAsync($"api/users/{userId}/cv", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>(_jsonOptions, cancellationToken) ?? new User();
        }

        public async Task UpdateProfilePicturePathAsync(int userId, string path, CancellationToken cancellationToken = default)
        {
            var response = await _http.PatchAsync(
                $"api/users/{userId}/profile-picture",
                JsonContent.Create(new { Path = path }, options: _jsonOptions),
                cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveProfilePicturePathAsync(int userId, CancellationToken cancellationToken = default)
        {
            var response = await _http.DeleteAsync($"api/users/{userId}/profile-picture", cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        private record XpResponse(int TotalExperiencePoints);
    }
}
