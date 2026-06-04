using System.Net.Http.Json;
using PussyCats.App.Dtos.TI;

namespace PussyCats.App.Services.TI;

public interface ITiApplicantService
{
    Task<List<TiApplicantDto>> GetByJobAsync(int jobId);
    Task<TiApplicantDto?> CreateAsync(TiApplicantDto dto);
    Task<bool> HasUserAppliedAsync(int jobId, int userId);
}

public class TiApplicantService : ITiApplicantService
{
    private readonly HttpClient http;

    public TiApplicantService(HttpClient http) => this.http = http;
    

    public async Task<List<TiApplicantDto>> GetByJobAsync(int jobId)
    {
        var response = await http.GetAsync($"api/applicants/byjob/{jobId}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiApplicantDto>>() ?? new();
    }

    public async Task<TiApplicantDto?> CreateAsync(TiApplicantDto dto)
    {
        var response = await http.PostAsJsonAsync("api/applicants", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TiApplicantDto>();
    }

    public async Task<bool> HasUserAppliedAsync(int jobId, int userId)
    {
        var applicants = await GetByJobAsync(jobId);
        return applicants.Any(a => a.UserId == userId);
    }
}
