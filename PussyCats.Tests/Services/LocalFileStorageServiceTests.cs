using FluentAssertions;
using NSubstitute;
using PussyCats.App.RepositoryProxies;
using PussyCats.App.Services;

namespace PussyCats.Tests.Services;

public class LocalFileStorageServiceTests
{
    private readonly IFilesProxy filesProxy = Substitute.For<IFilesProxy>();
    private readonly LocalFileStorageService service;

    public LocalFileStorageServiceTests()
    {
        service = new LocalFileStorageService(filesProxy);
    }

    [Fact]
    public async Task SaveFileAsync_delegates_upload_to_files_proxy()
    {
        filesProxy.UploadAsync(Arg.Any<Stream>(), "x.pdf", Arg.Any<CancellationToken>())
            .Returns("uploads/x.pdf");
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var result = await service.SaveFileAsync(stream, "x.pdf");

        result.Should().Be("uploads/x.pdf");
        await filesProxy.Received(1).UploadAsync(stream, "x.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteFileAsync_delegates_to_files_proxy()
    {
        await service.DeleteFileAsync("uploads/x.pdf");

        await filesProxy.Received(1).DeleteAsync("uploads/x.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public void GetFilePath_returns_proxy_resolved_url()
    {
        filesProxy.GetUrl("uploads/x.pdf").Returns("https://api/api/files/x.pdf");

        service.GetFilePath("uploads/x.pdf").Should().Be("https://api/api/files/x.pdf");
    }

    [Fact]
    public void GetFilePath_throws_for_null_path()
    {
        Action act = () => service.GetFilePath(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
