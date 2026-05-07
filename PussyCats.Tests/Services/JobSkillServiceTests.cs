using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.Services;

public class JobSkillServiceTests
{
    private readonly FakeJobSkillRepository repo;
    private readonly JobSkillService service;

    public JobSkillServiceTests()
    {
        repo = new FakeJobSkillRepository();
        service = new JobSkillService(repo);
    }

    [Fact]
    public async Task GetByIdAsync_EntryExists_ReturnsSeededEntry()
    {
        repo.Seed(new JobSkill { JobId = 1, SkillId = 2, RequiredLevel = 60 });

        var result = await service.GetByIdAsync(1, 2);

        result.Should().NotBeNull();
        result!.RequiredLevel.Should().Be(60);
    }

    [Fact]
    public async Task GetByIdAsync_EntryIsMissing_ReturnsNull()
    {
        var result = await service.GetByIdAsync(99, 99);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultipleEntriesExist_ReturnsEveryEntry()
    {
        repo.Seed(
            new JobSkill { JobId = 1, SkillId = 1, RequiredLevel = 50 },
            new JobSkill { JobId = 1, SkillId = 2, RequiredLevel = 70 },
            new JobSkill { JobId = 2, SkillId = 1, RequiredLevel = 60 });

        (await service.GetAllAsync()).Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByJobIdAsync_FilteredByJobId_ReturnsOnlyEntriesForThatJob()
    {
        repo.Seed(
            new JobSkill { JobId = 1, SkillId = 1, RequiredLevel = 50 },
            new JobSkill { JobId = 2, SkillId = 1, RequiredLevel = 60 });

        var result = await service.GetByJobIdAsync(1);

        result.Should().HaveCount(1);
        result[0].JobId.Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_ValidEntryProvided_PersistsEntry()
    {
        var entry = new JobSkill { JobId = 1, SkillId = 1, RequiredLevel = 50 };

        await service.AddAsync(entry);

        var fetched = await service.GetByIdAsync(1, 1);
        fetched.Should().BeEquivalentTo(entry);
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntryModified_ReplacesExistingEntryInStore()
    {
        repo.Seed(new JobSkill { JobId = 1, SkillId = 1, RequiredLevel = 50 });

        await service.UpdateAsync(new JobSkill { JobId = 1, SkillId = 1, RequiredLevel = 90 });

        var fetched = await service.GetByIdAsync(1, 1);
        fetched!.RequiredLevel.Should().Be(90);
    }

    [Fact]
    public async Task RemoveAsync_EntryExists_DeletesEntryFromStore()
    {
        repo.Seed(new JobSkill { JobId = 1, SkillId = 1, RequiredLevel = 50 });

        await service.RemoveAsync(1, 1);

        (await service.GetByIdAsync(1, 1)).Should().BeNull();
    }
}