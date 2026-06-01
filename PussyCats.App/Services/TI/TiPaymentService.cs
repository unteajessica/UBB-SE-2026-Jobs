using System.Net.Http.Json;
using PussyCats.App.Dtos.TI;

namespace PussyCats.App.Services.TI;

public interface ITiPaymentService
{
    Task<List<TiJobPaymentInfoDto>> GetPaidJobsInfoAsync(string? jobType = null, string? experienceLevel = null);
    Task<string> ProcessPaymentAsync(int jobId, decimal amount, string cardHolder, string cardNumber, string expDate, string cvv);
}

public class TiPaymentService : ITiPaymentService
{
    private readonly HttpClient http;

    public TiPaymentService(HttpClient http) => this.http = http;

    public async Task<List<TiJobPaymentInfoDto>> GetPaidJobsInfoAsync(string? jobType = null, string? experienceLevel = null)
    {
        var query = string.Empty;
        if (!string.IsNullOrEmpty(jobType) || !string.IsNullOrEmpty(experienceLevel))
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(jobType)) parts.Add($"jobType={Uri.EscapeDataString(jobType)}");
            if (!string.IsNullOrEmpty(experienceLevel)) parts.Add($"experienceLevel={Uri.EscapeDataString(experienceLevel)}");
            query = "?" + string.Join("&", parts);
        }

        var response = await http.GetAsync($"api/payment/jobs{query}");
        if (!response.IsSuccessStatusCode) return new();
        return await response.Content.ReadFromJsonAsync<List<TiJobPaymentInfoDto>>() ?? new();
    }

    public async Task<string> ProcessPaymentAsync(int jobId, decimal amount, string cardHolder, string cardNumber, string expDate, string cvv)
    {
        var payload = new { JobId = jobId, Amount = amount, CardHolderName = cardHolder, CardNumber = cardNumber, ExpDate = expDate, Cvv = cvv };
        var response = await http.PostAsJsonAsync("api/payment", payload);
        if (response.IsSuccessStatusCode) return string.Empty;
        return await response.Content.ReadAsStringAsync();
    }
}
