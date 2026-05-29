using Tests_and_Interviews_API.DTOs;

namespace Tests_and_Interviews_API.Services.Interfaces
{
    /// <summary>
    /// Handles authentication operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Validates credentials and returns a JWT if valid.
        /// </summary>
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);

        /// <summary>
        /// Registers a new user and returns a JWT.
        /// </summary>
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    }
}