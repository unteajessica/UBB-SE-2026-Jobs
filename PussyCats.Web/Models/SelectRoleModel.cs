using PussyCats.Library.Domain.Enums;

namespace PussyCats.Web.Models;

public class SelectRoleModel
{
    public List<RoleOption> TopRoles { get; set; } = new();
    public Dictionary<int, int> Answers { get; set; } = new();
    public JobRole SelectedRole { get; set; }
}

public class RoleOption
{
    public JobRole Role { get; set; }
    public string DisplayName { get; set; } = "";
    public double Score { get; set; }
}
