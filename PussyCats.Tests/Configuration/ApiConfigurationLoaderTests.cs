using PussyCats.App.Configuration;

namespace PussyCats.Tests.Configuration;

public class ApiConfigurationLoaderTests : IDisposable
{
    private readonly string tempDirectory = Directory.CreateTempSubdirectory("pussycats-api-config-").FullName;

    [Fact]
    public void Load_NoConfigExists_ReturnsDefaultBaseUrl()
    {
        var configuration = ApiConfigurationLoader.Load(tempDirectory);

        Assert.Equal("https://localhost:7134", configuration.BaseUrl);
    }

    [Fact]
    public void Load_BundledAppSettingsFileExists_ReadsBaseUrl()
    {
        File.WriteAllText(
            Path.Combine(tempDirectory, "appsettings.json"),
            """
            {
              "Api": {
                "BaseUrl": "http://localhost:5000"
              }
            }
            """);

        var configuration = ApiConfigurationLoader.Load(tempDirectory);

        Assert.Equal("http://localhost:5000", configuration.BaseUrl);
    }

    [Fact]
    public void Load_LocalAppSettingsFileExists_PrefersLocalFile()
    {
        File.WriteAllText(
            Path.Combine(tempDirectory, "appsettings.json"),
            """
            {
              "Api": {
                "BaseUrl": "http://localhost:5000"
              }
            }
            """);
        File.WriteAllText(
            Path.Combine(tempDirectory, "appsettings.local.json"),
            """
            {
              "Api": {
                "BaseUrl": "http://localhost:6000"
              }
            }
            """);

        var configuration = ApiConfigurationLoader.Load(tempDirectory);

        Assert.Equal("http://localhost:6000", configuration.BaseUrl);
    }

    public void Dispose()
    {
        Directory.Delete(tempDirectory, recursive: true);
    }
}