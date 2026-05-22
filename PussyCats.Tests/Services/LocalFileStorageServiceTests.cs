using FluentAssertions;
using NSubstitute;
using PussyCats.App.RepositoryProxies;
using PussyCats.Library.Services.FileStorage;
using PussyCats_App.Services.LocalFileStorageService;

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
    public async Task SaveFileAsync_ValidStreamProvided_DelegatesUploadToFilesProxy()
    {
        filesProxy.UploadAsync(Arg.Any<Stream>(), "x.pdf", Arg.Any<CancellationToken>())
            .Returns("uploads/x.pdf");
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var result = await service.SaveFileAsync(stream, "x.pdf");

        result.Should().Be("uploads/x.pdf");
        await filesProxy.Received(1).UploadAsync(stream, "x.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteFileAsync_FilePathProvided_DelegatesToFilesProxy()
    {
        await service.DeleteFileAsync("uploads/x.pdf");

        await filesProxy.Received(1).DeleteAsync("uploads/x.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OpenReadAsync_delegates_to_files_proxy()
    {
        using var stream = new MemoryStream(new byte[] { 4, 5, 6 });
        filesProxy.DownloadAsync("uploads/x.pdf", Arg.Any<CancellationToken>()).Returns(stream);

        var result = await service.OpenReadAsync("uploads/x.pdf");

        result.Should().BeSameAs(stream);
        await filesProxy.Received(1).DownloadAsync("uploads/x.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public void GetUrl_RelativePathProvided_ReturnsProxyResolvedUrl()
    {
        filesProxy.GetUrl("uploads/x.pdf").Returns("https://api/api/files/x.pdf");

        service.GetUrl("uploads/x.pdf").Should().Be("https://api/api/files/x.pdf");
    }

    [Fact]
    public void GetUrl_PathIsNull_ThrowsArgumentNullException()
    {
        Action act = () => service.GetUrl(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
