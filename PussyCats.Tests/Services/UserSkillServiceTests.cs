using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Tests.Fakes;

namespace PussyCats.Tests.Services;

public class UserSkillServiceTests
{
    private readonly FakeUserSkillRepository userSkillRepository;
    private readonly UserSkillService userSkillService;

    public UserSkillServiceTests()
    {
        userSkillRepository = new FakeUserSkillRepository();
        userSkillService = new UserSkillService(userSkillRepository);
    }


    [Fact]
    public async Task AddAsync_ValidEntryProvided_PersistsEntryAndReturnsIt()
    {
        var entry = new UserSkill { User = new User { UserId = 1 }, Skill = new Skill { SkillId = 1 } , Score = 75 };

        var result = await userSkillService.AddAsync(entry);

        result.Should().BeSameAs(entry);
        (await userSkillService.GetByIdAsync(1, 1)).Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntryModified_ReplacesExistingEntry()
    {
        userSkillRepository.Seed(new UserSkill { User = new User { UserId = 1 }, Skill = new Skill { SkillId = 1 }, Score = 50 });

        await userSkillService.UpdateAsync(new UserSkill { User = new User { UserId = 1 }, Skill = new Skill { SkillId = 1 }, Score = 95 });

        (await userSkillService.GetByIdAsync(1, 1))!.Score.Should().Be(95);
    }

    [Fact]
    public async Task RemoveAsync_UserSkillExists_DeletesUserSkill()
    {
        userSkillRepository.Seed(new UserSkill { User = new User { UserId = 1 }, Skill = new Skill { SkillId = 1 }, Score = 70 });

        await userSkillService.RemoveAsync(1, 1);

        (await userSkillService.GetByIdAsync(1, 1)).Should().BeNull();
    }
}