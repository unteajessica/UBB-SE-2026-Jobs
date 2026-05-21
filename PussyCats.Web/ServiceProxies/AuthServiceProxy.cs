using System.Net.Http.Json;

namespace PussyCats.Web.ServiceProxies;

public class AuthServiceProxy
{
    private readonly HttpClient http;

    public AuthServiceProxy(HttpClient http) => this.http = http;

    public Task<HttpResponseMessage> LoginAsync(string email, string password, CancellationToken ct = default)
        => http.PostAsJsonAsync("api/auth/login", new { email, password }, ct);

    public Task<HttpResponseMessage> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken ct = default)
        => http.PostAsJsonAsync("api/auth/register", new { email, password, firstName, lastName }, ct);
}
