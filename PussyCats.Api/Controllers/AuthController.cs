using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PussyCats.Library.Domain;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.Users;

namespace PussyCats.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService users;
    private readonly IConfiguration configuration;

    public AuthController(IUserService users, IConfiguration configuration)
    {
        this.users = users;
        this.configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return Unauthorized();

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized();

        return Ok(ToAuthResponse(user));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (await users.ExistsWithEmailAsync(request.Email, cancellationToken))
            return Conflict("Email already registered.");

        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow,
        };
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        var saved = await users.AddAsync(user, cancellationToken);
        return Ok(ToAuthResponse(saved));
    }

    private AuthResponse ToAuthResponse(User user)
    {
        return new AuthResponse(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            GenerateJwt(user));
    }

    private string GenerateJwt(User user)
    {
        var key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Missing JWT signing key configuration.");
        }

        var issuer = configuration["Jwt:Issuer"] ?? "PussyCats.Api";
        var audience = configuration["Jwt:Audience"] ?? "PussyCats.Clients";
        var expiresMinutes = configuration.GetValue("Jwt:ExpiresMinutes", 120);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("UserId", user.UserId.ToString()),
            new Claim("Email", user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
}
