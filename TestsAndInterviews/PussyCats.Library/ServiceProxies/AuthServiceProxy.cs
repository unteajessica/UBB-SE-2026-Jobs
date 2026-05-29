using System.Net.Http.Json;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Auth;

namespace PussyCats.Library.ServiceProxies;

public class AuthServiceProxy : IAuthService
{
    private readonly HttpClient http;

    public AuthServiceProxy(HttpClient http) => this.http = http;

    public async Task<AuthServiceResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var response = await LoginRawAsync(email, password, cancellationToken).ConfigureAwait(false);
        return await ToResultAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AuthServiceResult> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        var response = await RegisterRawAsync(email, password, firstName, lastName, cancellationToken).ConfigureAwait(false);
        return await ToResultAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public Task<HttpResponseMessage> LoginRawAsync(string email, string password, CancellationToken cancellationToken = default)
        => http.PostAsJsonAsync("api/auth/login", new { email, password }, cancellationToken);

    public Task<HttpResponseMessage> RegisterRawAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
        => http.PostAsJsonAsync("api/auth/register", new { email, password, firstName, lastName }, cancellationToken);

    private static async Task<AuthServiceResult> ToResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return new AuthServiceResult(false, null, response.StatusCode, error);
        }

        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return new AuthServiceResult(payload is not null, payload, response.StatusCode, null);
    }
}
