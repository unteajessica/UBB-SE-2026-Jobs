using System.Net.Http.Json;
using PussyCats.App.Dtos.TI;

namespace PussyCats.App.Services.TI;

public interface ITiSlotsService
{
    Task<List<TiSlotDto>> GetAvailableAsync(DateTime date);
    Task<List<TiSlotDto>> GetByRecruiterAsync(int recruiterId, DateTime date);
    Task<bool> BookSlotAsync(int slotId, int candidateId);
    Task<List<TiInterviewSessionDto>> GetScheduledSessionsAsync();
    Task<List<TiInterviewSessionDto>> GetSessionsByStatusAsync(string status);
}

public class TiSlotsService : ITiSlotsService
{
    private readonly HttpClient http;

    public TiSlotsService(HttpClient http) => this.http = http;

    public async Task<List<TiSlotDto>> GetAvailableAsync(DateTime date)
    {
        var response = await http.GetAsync($"api/slots/available?date={date:yyyy-MM-dd}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiSlotDto>>() ?? new();
    }

    public async Task<List<TiSlotDto>> GetByRecruiterAsync(int recruiterId, DateTime date)
    {
        var response = await http.GetAsync($"api/slots/recruiter/{recruiterId}?date={date:yyyy-MM-dd}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiSlotDto>>() ?? new();
    }

    public async Task<bool> BookSlotAsync(int slotId, int candidateId)
    {
        var response = await http.PostAsJsonAsync($"api/bookings/{slotId}/confirm", new { CandidateId = candidateId });
        return response.IsSuccessStatusCode;
    }

    public async Task<List<TiInterviewSessionDto>> GetScheduledSessionsAsync()
    {
        var response = await http.GetAsync("api/interviewsessions/scheduled");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiInterviewSessionDto>>() ?? new();
    }

    public async Task<List<TiInterviewSessionDto>> GetSessionsByStatusAsync(string status)
    {
        var response = await http.GetAsync($"api/interviewsessions/status/{status}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiInterviewSessionDto>>() ?? new();
    }
}
