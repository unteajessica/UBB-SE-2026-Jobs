using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.Services;

public class UserSkillServiceTests
{
    private readonly FakeUserSkillRepository repo;
    private readonly UserSkillService service;

    public UserSkillServiceTests()
    {
        repo = new FakeUserSkillRepository();
        service = new UserSkillService(repo);
    }

    [Fact]
    public async Task GetByIdAsync_EntryExists_ReturnsSeededEntry()
    {
        repo.Seed(new UserSkill { UserId = 1, SkillId = 1, Score = 85 });

        var result = await service.GetByIdAsync(1, 1);

        result.Should().NotBeNull();
        result!.Score.Should().Be(85);
    }

    [Fact]
    public async Task GetByUserIdAsync_MultipleUsersExist_FiltersByUserId()
    {
        repo.Seed(
            new UserSkill { UserId = 1, SkillId = 1, Score = 80 },
            new UserSkill { UserId = 1, SkillId = 2, Score = 60 },
            new UserSkill { UserId = 2, SkillId = 1, Score = 90 });

        var result = await service.GetByUserIdAsync(1);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(userSkill => userSkill.UserId == 1);
    }

    [Fact]
    public async Task AddAsync_ValidEntryProvided_PersistsEntryAndReturnsIt()
    {
        var entry = new UserSkill { UserId = 1, SkillId = 1, Score = 75 };

        var result = await service.AddAsync(entry);

        result.Should().BeSameAs(entry);
        (await service.GetByIdAsync(1, 1)).Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntryModified_ReplacesExistingEntry()
    {
        repo.Seed(new UserSkill { UserId = 1, SkillId = 1, Score = 50 });

        await service.UpdateAsync(new UserSkill { UserId = 1, SkillId = 1, Score = 95 });

        (await service.GetByIdAsync(1, 1))!.Score.Should().Be(95);
    }

    [Fact]
    public async Task RemoveAsync_UserSkillExists_DeletesUserSkill()
    {
        repo.Seed(new UserSkill { UserId = 1, SkillId = 1, Score = 70 });

        await service.RemoveAsync(1, 1);

        (await service.GetByIdAsync(1, 1)).Should().BeNull();
    }
}