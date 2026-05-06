using System.Net.Http.Json;
using PussyCats.Library.Domain;
using PussyCats.Library.Repositories.PersonalityTests;

namespace PussyCats.App.RepositoryProxies;

public class PersonalityTestRepositoryProxy : IPersonalityTestRepository
{
    private readonly HttpClient http;

    public PersonalityTestRepositoryProxy(HttpClient http)
    {
        this.http = http;
    }

    public async Task<PersonalityTestResult?> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<PersonalityTestResult>(
            http,
            $"api/personality-tests?userId={userId}",
            ct).ConfigureAwait(false);
    }

    public async Task<PersonalityTestResult> AddAsync(PersonalityTestResult result, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("api/personality-tests", result, RepositoryProxyJson.Options, ct).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<PersonalityTestResult>(response, ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(PersonalityTestResult result, CancellationToken ct = default)
    {
        using var response = await http.PutAsJsonAsync(
            $"api/personality-tests/{result.PersonalityTestResultId}",
            result,
            RepositoryProxyJson.Options,
            ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int personalityTestResultId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/personality-tests/{personalityTestResultId}", ct).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
