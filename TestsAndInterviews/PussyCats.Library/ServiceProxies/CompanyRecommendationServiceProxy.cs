using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.CompanyRecommendationService;

namespace PussyCats.Library.ServiceProxies;

public class CompanyRecommendationServiceProxy : ICompanyRecommendationService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly HttpClient http;
    private List<UserApplicationResult> rankedApplicants = new List<UserApplicationResult>();
    private int currentApplicantIndex;

    public CompanyRecommendationServiceProxy(HttpClient http)
    {
        this.http = http;
    }

    public bool HasMore => currentApplicantIndex < rankedApplicants.Count;

    public async Task<IReadOnlyList<UserApplicationResult>> GetRankedApplicantsAsync(
        int companyId,
        CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<List<UserApplicationResult>>(
                   $"api/company-recommendations/companies/{companyId}/applicants",
                   JsonOptions,
                   cancellationToken)
               ?? new List<UserApplicationResult>();
    }

    public async Task<UserApplicationResult?> GetApplicantByMatchIdAsync(
        int companyId,
        int matchId,
        CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync(
            $"api/company-recommendations/companies/{companyId}/applicants/{matchId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserApplicationResult>(JsonOptions, cancellationToken);
    }

    public async Task LoadApplicantsAsync(int companyId, CancellationToken cancellationToken = default)
    {
        rankedApplicants = (await GetRankedApplicantsAsync(companyId, cancellationToken).ConfigureAwait(false)).ToList();
        currentApplicantIndex = 0;
    }

    public UserApplicationResult? GetNextApplicant()
    {
        if (currentApplicantIndex >= rankedApplicants.Count)
        {
            return null;
        }

        return rankedApplicants[currentApplicantIndex];
    }

    public void MoveToNext()
    {
        currentApplicantIndex++;
    }

    public void MoveToPrevious()
    {
        if (currentApplicantIndex > 0)
        {
            currentApplicantIndex--;
        }
    }

    public async Task<CompatibilityBreakdown?> GetBreakdownAsync(
        UserApplicationResult applicant,
        CancellationToken cancellationToken = default)
    {
        var response = await http.PostAsJsonAsync(
            "api/company-recommendations/breakdown",
            applicant,
            JsonOptions,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CompatibilityBreakdown>(JsonOptions, cancellationToken);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
