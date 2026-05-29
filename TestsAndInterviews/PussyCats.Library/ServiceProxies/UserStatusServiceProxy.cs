using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.UserStatusService;

namespace PussyCats.Library.ServiceProxies;

public class UserStatusServiceProxy : IUserStatusService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient http;

    public UserStatusServiceProxy(HttpClient http) => this.http = http;

    public async Task<IReadOnlyList<ApplicationCardModel>> GetApplicationsForUserAsync(int userId, CancellationToken cancellationToken = default)
        => await http.GetFromJsonAsync<List<ApplicationCardModel>>(
               $"api/user-status/{userId}/applications", JsonOptions, cancellationToken)
           ?? new List<ApplicationCardModel>();
}
