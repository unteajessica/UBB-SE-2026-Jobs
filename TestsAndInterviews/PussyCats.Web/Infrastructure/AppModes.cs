namespace PussyCats.Web.Infrastructure;

public static class AppModes
{
    public const string User = "User";
    public const string Company = "Company";
    public const string Developer = "Developer";

    public static bool IsValid(string? mode)
        => string.Equals(mode, User, StringComparison.OrdinalIgnoreCase)
           || string.Equals(mode, Company, StringComparison.OrdinalIgnoreCase)
           || string.Equals(mode, Developer, StringComparison.OrdinalIgnoreCase);
}
