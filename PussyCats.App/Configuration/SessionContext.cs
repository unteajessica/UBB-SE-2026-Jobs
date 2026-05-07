using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.Configuration;

public sealed class SessionContext
{
    public int UserId { get; set; } = 1;
    public int? CompanyId { get; set; } = 2;
    public int? DeveloperId { get; set; } = 1;
    public AppMode Mode { get; set; } = AppMode.Candidate;
}
