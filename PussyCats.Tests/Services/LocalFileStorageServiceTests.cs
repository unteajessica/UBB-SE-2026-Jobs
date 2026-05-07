using FluentAssertions;
using PussyCats.App.Services;

namespace PussyCats.Tests.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string tempDir;
    private readonly LocalFileStorageService service;

    public LocalFileStorageServiceTests()
    {
        tempDir = Path.Combine(Path.GetTempPath(), $"local-fs-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        service = new LocalFileStorageService(tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Constructor_creates_directory_when_missing()
    {
        var newDir = Path.Combine(Path.GetTempPath(), $"new-{Guid.NewGuid():N}");
        try
        {
            _ = new LocalFileStorageService(newDir);
            Directory.Exists(newDir).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(newDir))
            {
                Directory.Delete(newDir, recursive: true);
            }
        }
    }

    [Fact]
    public void SaveFile_throws_NotImplementedException_per_phase_5_decision()
    {
        // Phase 5 design: silent disk writes are masked behind /api/files.
        using var stream = new MemoryStream();
        Action act = () => service.SaveFile(stream, "x.pdf");

        act.Should().Throw<NotImplementedException>()
            .WithMessage("*Phase 5 routes file uploads*");
    }

    [Fact]
    public void DeleteFile_silently_returns_for_blank_path()
    {
        Action act = () => service.DeleteFile("");

        act.Should().NotThrow();
    }

    [Fact]
    public void GetFilePath_throws_when_file_missing()
    {
        Action act = () => service.GetFilePath("does-not-exist.pdf");

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void GetFilePath_throws_for_null_path()
    {
        Action act = () => service.GetFilePath(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
