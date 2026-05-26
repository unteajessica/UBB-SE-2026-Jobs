using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Services.CompletenessService;

namespace PussyCats.Library.ServiceProxies;

public class CompletenessServiceProxy : ICompletenessService
{
    private readonly HttpClient http;
    private int? cachedUserId;
    private CompletenessResponse? cachedResponse;

    public CompletenessServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public int CalculateCompleteness(User? user)
    {
        return GetCompleteness(user).Percentage;
    }

    public string GetNextEmptyFieldPrompt(User? user)
    {
        return GetCompleteness(user).NextPrompt;
    }

    private CompletenessResponse GetCompleteness(User? user)
    {
        if (user is null || user.UserId <= 0)
        {
            return new CompletenessResponse(0, string.Empty);
        }

        if (cachedUserId == user.UserId && cachedResponse is not null)
        {
            return cachedResponse;
        }

        cachedResponse = http.GetFromJsonAsync<CompletenessResponse>(
                $"api/users/{user.UserId}/completeness")
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult()
            ?? new CompletenessResponse(0, string.Empty);
        cachedUserId = user.UserId;
        return cachedResponse;
    }

    private sealed record CompletenessResponse(int Percentage, string NextPrompt);
}
