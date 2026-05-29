namespace PussyCats.App.Configuration;

public static class ApiConfigurationLoader
{
    private const string DefaultBaseUrl = "https://localhost:7134";

    public static ApiConfiguration Load(string? baseDirectory = null)
    {
        var directory = baseDirectory ?? AppContext.BaseDirectory;
        var localConfigPath = Path.Combine(directory, "appsettings.local.json");
        var bundledConfigPath = Path.Combine(directory, "appsettings.json");

        return TryLoad(localConfigPath)
            ?? TryLoad(bundledConfigPath)
            ?? new ApiConfiguration(DefaultBaseUrl);
    }

    private static ApiConfiguration? TryLoad(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
        if (!document.RootElement.TryGetProperty("Api", out var apiSection)
            || !apiSection.TryGetProperty("BaseUrl", out var baseUrlProperty))
        {
            return null;
        }

        var baseUrl = baseUrlProperty.GetString();
        return string.IsNullOrWhiteSpace(baseUrl)
            ? null
            : new ApiConfiguration(baseUrl.Trim());
    }
}
