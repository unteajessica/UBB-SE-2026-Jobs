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

    public async Task<PersonalityTestResult?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await RepositoryProxyJson.GetOrNullAsync<PersonalityTestResult>(
            http,
            $"api/personality-tests?userId={userId}",
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<PersonalityTestResult> AddAsync(PersonalityTestResult result, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/personality-tests", result, RepositoryProxyJson.Options, cancellationToken).ConfigureAwait(false);
        return await RepositoryProxyJson.ReadRequiredAsync<PersonalityTestResult>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(PersonalityTestResult result, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync(
            $"api/personality-tests/{result.PersonalityTestResultId}",
            result,
            RepositoryProxyJson.Options,
            cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }

    public async Task RemoveAsync(int personalityTestResultId, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/personality-tests/{personalityTestResultId}", cancellationToken).ConfigureAwait(false);
        await RepositoryProxyJson.SendAndIgnoreNotFoundAsync(response).ConfigureAwait(false);
    }
}
