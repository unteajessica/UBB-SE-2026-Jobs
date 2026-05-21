using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PussyCats.Library.Domain;
using PussyCats.Library.Persistence;
using PussyCats.Library.Services.Users;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService users;
    private readonly PussyCatsDbContext db;

    public AuthController(IUserService users, PussyCatsDbContext db)
    {
        this.users = users;
        this.db = db;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null)
            return Unauthorized();

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized();

        return Ok(new UserInfoResponse(user.UserId, user.Email, user.FirstName, user.LastName));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var exists = await db.Users.AsNoTracking()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (exists)
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
        return Ok(new UserInfoResponse(saved.UserId, saved.Email, saved.FirstName, saved.LastName));
    }

    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
    public record UserInfoResponse(int UserId, string Email, string FirstName, string LastName);
}
