using FluentAssertions;
using PussyCats.App.Configuration;

namespace PussyCats.Tests.Configuration;

public class ApiConfigurationLoaderTests : IDisposable
{
    private readonly string tempDirectory = Directory.CreateTempSubdirectory("pussycats-api-config-").FullName;

    [Fact]
    public void Load_NoConfigExists_ReturnsDefaultBaseUrl()
    {
        var configuration = ApiConfigurationLoader.Load(tempDirectory);

        configuration.BaseUrl.Should().Be("https://localhost:7134");
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

        configuration.BaseUrl.Should().Be("http://localhost:5000");
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

        configuration.BaseUrl.Should().Be("http://localhost:6000");
    }

    public void Dispose()
    {
        Directory.Delete(tempDirectory, recursive: true);
    }
}