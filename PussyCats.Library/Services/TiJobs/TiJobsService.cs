using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;

namespace PussyCats.Library.Services.TiJobs;

public interface ITiJobsService
{
    Task<List<TiJobPostingDto>> GetAllJobsAsync();
    Task<TiJobPostingDto?> GetByIdAsync(int jobId);
    Task<List<TiSkillDto>> GetAllSkillsAsync();
    Task<int> AddJobAsync(Job job);
    Task<bool> UpdateJobAsync(int jobId, TiJobPostingDto dto);
    Task<bool> DeleteJobAsync(int jobId);
}

public class TiJobsService : ITiJobsService
{
    private readonly HttpClient http;

    public TiJobsService(HttpClient http) => this.http = http;

    public async Task<List<TiJobPostingDto>> GetAllJobsAsync()
    {
        var response = await http.GetAsync("api/jobs");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiJobPostingDto>>() ?? new();
    }

    public async Task<TiJobPostingDto?> GetByIdAsync(int jobId)
    {
        var all = await GetAllJobsAsync();
        return all.Find(job => job.JobId == jobId);
    }

    public async Task<List<TiSkillDto>> GetAllSkillsAsync()
    {
        var response = await http.GetAsync("api/jobs/skills");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiSkillDto>>() ?? new();
    }

    public async Task<int> AddJobAsync(Job job)
    {
        var response = await http.PostAsJsonAsync("api/jobs", job);
        response.EnsureSuccessStatusCode();
        var saved = await response.Content.ReadFromJsonAsync<Job>();
        return saved!.JobId;
    }

    public async Task<bool> UpdateJobAsync(int jobId, TiJobPostingDto dto)
    {
        var response = await http.PutAsJsonAsync($"api/jobs/{jobId}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteJobAsync(int jobId)
    {
        var response = await http.DeleteAsync($"api/jobs/{jobId}");
        return response.IsSuccessStatusCode;
    }
}
