using FluentAssertions;
using NSubstitute;
using PussyCats.App.RepositoryProxies;
using PussyCats.App.Services;

namespace PussyCats.Tests.Services;

public class ImageStorageServiceTests
{
    private readonly IFilesProxy filesProxy = Substitute.For<IFilesProxy>();
    private readonly ImageStorageService service;

    public ImageStorageServiceTests()
    {
        service = new ImageStorageService(filesProxy);
    }

    [Fact]
    public async Task SaveImageAsync_rejects_unsupported_extension_before_uploading()
    {
        using var stream = new MemoryStream();

        Func<Task> act = () => service.SaveImageAsync(stream, "x.gif");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Unsupported file type*");
        await filesProxy.DidNotReceiveWithAnyArgs().UploadAsync(default!, default!, default);
    }

    [Fact]
    public async Task SaveImageAsync_uploads_through_files_proxy_with_normalized_name()
    {
        filesProxy.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("uploads/upload.png");
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var result = await service.SaveImageAsync(stream, "Photo.PNG");

        result.Should().Be("uploads/upload.png");
        await filesProxy.Received(1).UploadAsync(stream, "upload.png", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteImageAsync_delegates_to_files_proxy()
    {
        await service.DeleteImageAsync("uploads/x.png");

        await filesProxy.Received(1).DeleteAsync("uploads/x.png", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveImageAsync_throws_when_stream_exceeds_20mb()
    {
        using var stream = new MemoryStream(new byte[20 * 1024 * 1024 + 1]);

        Func<Task> act = () => service.SaveImageAsync(stream, "x.png");

        await act.Should().ThrowAsync<InvalidOperationException>()
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
