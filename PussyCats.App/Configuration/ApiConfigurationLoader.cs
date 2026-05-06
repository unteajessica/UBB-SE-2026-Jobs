namespace PussyCats.App.Configuration;

public static class ApiConfigurationLoader
{
    public static ApiConfiguration Load()
    {
        // TODO Phase 8: read from bundled config file.
        return new ApiConfiguration("https://localhost:7000");
    }
}
