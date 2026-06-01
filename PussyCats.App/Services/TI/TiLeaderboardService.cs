using System.Net.Http.Json;
using PussyCats.App.Dtos.TI;

namespace PussyCats.App.Services.TI;

public interface ITiLeaderboardService
{
    Task RecalculateAsync(int testId);
    Task<List<TiLeaderboardEntryDto>> GetByTestIdAsync(int testId);
    Task<List<TiLeaderboardEntryDto>> GetTopByTestIdAsync(int testId, int limit = 3);
    Task<TiLeaderboardEntryDto?> GetUserEntryAsync(int testId, int userId);
}

public class TiLeaderboardService : ITiLeaderboardService
{
    private readonly HttpClient http;

    public TiLeaderboardService(HttpClient http) => this.http = http;

    public async Task RecalculateAsync(int testId)
    {
        await http.PostAsync($"api/leaderboard/recalculate/{testId}", null);
    }

    public async Task<List<TiLeaderboardEntryDto>> GetByTestIdAsync(int testId)
    {
        await RecalculateAsync(testId);
        var response = await http.GetAsync($"api/leaderboard/bytest/{testId}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiLeaderboardEntryDto>>() ?? new();
    }

    public async Task<List<TiLeaderboardEntryDto>> GetTopByTestIdAsync(int testId, int limit = 3)
    {
        await RecalculateAsync(testId);
        var response = await http.GetAsync($"api/leaderboard/bytest/{testId}/top/{limit}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiLeaderboardEntryDto>>() ?? new();
    }

    public async Task<TiLeaderboardEntryDto?> GetUserEntryAsync(int testId, int userId)
    {
        await RecalculateAsync(testId);
        var response = await http.GetAsync($"api/leaderboard/bytest/{testId}/byuser/{userId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TiLeaderboardEntryDto>();
    }
}
