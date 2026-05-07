using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class JobServiceTests
{
    private readonly FakeJobRepository repo = new();
    private readonly JobService service;

    public JobServiceTests()
    {
        service = new JobService(repo);
    }

    [Fact]
    public async Task GetByIdAsync_returns_job()
    {
        repo.Seed(new JobBuilder().WithId(1).WithTitle("Backend Engineer").Build());

        var job = await service.GetByIdAsync(1);

        job.Should().NotBeNull();
        job!.JobTitle.Should().Be("Backend Engineer");
    }

    [Fact]
    public async Task GetAllAsync_returns_every_job()
    {
        repo.Seed(
            new JobBuilder().WithId(1).Build(),
            new JobBuilder().WithId(2).Build());

        (await service.GetAllAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByCompanyIdAsync_filters_by_company()
    {
        repo.Seed(
            new JobBuilder().WithId(1).WithCompanyId(10).Build(),
            new JobBuilder().WithId(2).WithCompanyId(10).Build(),
            new JobBuilder().WithId(3).WithCompanyId(20).Build());

        var jobs = await service.GetByCompanyIdAsync(10);

        jobs.Should().HaveCount(2);
        jobs.Should().OnlyContain(j => j.CompanyId == 10);
    }

    [Fact]
    public async Task AddAsync_persists_and_assigns_id()
    {
        var job = new JobBuilder().WithId(0).Build();

        var saved = await service.AddAsync(job);

        saved.JobId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateAsync_replaces_job()
    {
        var job = new JobBuilder().WithId(1).WithTitle("Old").Build();
        repo.Seed(job);
        job.JobTitle = "New";

        await service.UpdateAsync(job);

        (await service.GetByIdAsync(1))!.JobTitle.Should().Be("New");
    }

    [Fact]
    public async Task RemoveAsync_deletes_job()
    {
        repo.Seed(new JobBuilder().WithId(1).Build());

        await service.RemoveAsync(1);

        (await service.GetByIdAsync(1)).Should().BeNull();
    }
}
