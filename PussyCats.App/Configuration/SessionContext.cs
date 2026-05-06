using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.Configuration;

public sealed class SessionContext
{
    public int UserId { get; set; }
    public int? CompanyId { get; set; }
    public AppMode Mode { get; set; } = AppMode.Candidate;
}
