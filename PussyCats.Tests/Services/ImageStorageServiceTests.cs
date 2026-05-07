using FluentAssertions;
using PussyCats.App.Services;

namespace PussyCats.Tests.Services;

public class ImageStorageServiceTests : IDisposable
{
    private readonly string tempDir;
    private readonly ImageStorageService service;

    public ImageStorageServiceTests()
    {
        tempDir = Path.Combine(Path.GetTempPath(), $"image-fs-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        service = new ImageStorageService(tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void SaveImage_rejects_unsupported_extension_before_uploading()
    {
        using var stream = new MemoryStream();
        Action act = () => service.SaveImage(stream, "x.gif");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unsupported file type*");
    }

    [Fact]
    public void DeleteImage_silently_returns_for_blank_path()
    {
        Action act = () => service.DeleteImage("");

        act.Should().NotThrow();
    }

    [Fact]
    public void CheckFileSize_throws_when_stream_exceeds_20mb()
    {
        // 20 MB + 1 byte
        using var stream = new MemoryStream(new byte[20 * 1024 * 1024 + 1]);

        Action act = () => service.CheckFileSize(stream);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*20 MB*");
    }

    [Fact]
    public void CheckFileSize_passes_when_stream_under_20mb()
    {
        using var stream = new MemoryStream(new byte[10 * 1024 * 1024]);

        Action act = () => service.CheckFileSize(stream);

        act.Should().NotThrow();
    }
}
