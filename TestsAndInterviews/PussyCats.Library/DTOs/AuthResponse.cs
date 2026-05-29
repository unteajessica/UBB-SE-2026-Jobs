using System.Net;

namespace PussyCats.Library.DTOs;

public sealed record AuthResponse(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string Token);

public sealed record AuthServiceResult(
    bool Succeeded,
    AuthResponse? Response,
    HttpStatusCode StatusCode,
    string? ErrorMessage);
