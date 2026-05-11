using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.Configuration;

public sealed class SessionContext
{
    public int UserId { get; set; } = 1;
    public int? CompanyId { get; set; } = 3;
    //public Company? Company { get; set; }
    public int? DeveloperId { get; set; } = 1;
    public AppMode Mode { get; set; } = AppMode.Candidate;
}
