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

    private const string DefaultTiBaseUrl = "http://localhost:5179/";

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
        if (string.IsNullOrWhiteSpace(baseUrl))
            return null;

        var tiBaseUrl = DefaultTiBaseUrl;
        if (apiSection.TryGetProperty("TiBaseUrl", out var tiBaseUrlProperty))
        {
            var raw = tiBaseUrlProperty.GetString();
            if (!string.IsNullOrWhiteSpace(raw))
                tiBaseUrl = raw.Trim();
        }

        return new ApiConfiguration(baseUrl.Trim(), tiBaseUrl);
    }
}
