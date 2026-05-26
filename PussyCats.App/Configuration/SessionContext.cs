using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.App.Configuration;

public sealed class SessionContext
{
    public int UserId { get; set; }
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string JwtToken { get; private set; } = string.Empty;
    public bool IsAuthenticated => UserId > 0 && !string.IsNullOrWhiteSpace(JwtToken);
    public int? CompanyId { get; set; }
    public int? DeveloperId { get; set; }
    public AppMode Mode { get; set; } = AppMode.Candidate;

    public void SignIn(AuthResponse response)
    {
        UserId = response.UserId;
        Email = response.Email;
        DisplayName = $"{response.FirstName} {response.LastName}".Trim();
        JwtToken = response.Token;
        Mode = AppMode.Candidate;
    }

    public void SignOut()
    {
        UserId = 0;
        Email = string.Empty;
        DisplayName = string.Empty;
        JwtToken = string.Empty;
        CompanyId = null;
        DeveloperId = null;
        Mode = AppMode.Candidate;
    }
}
