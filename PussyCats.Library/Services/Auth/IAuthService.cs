using PussyCats.Library.DTOs;

namespace PussyCats.Library.Services.Auth;

public interface IAuthService
{
    Task<AuthServiceResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AuthServiceResult> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);
}
