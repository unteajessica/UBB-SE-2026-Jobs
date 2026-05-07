using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class UserProfileServiceTests
{
    private readonly FakeUserRepository userRepo = new();
    private readonly FakeSkillTestRepository skillTestRepo = new();
    private readonly UserProfileService service;

    public UserProfileServiceTests()
    {
        service = new UserProfileService(userRepo, skillTestRepo);
    }

    [Fact]
    public async Task GetProfileAsync_returns_user()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());

        (await service.GetProfileAsync(1)).Should().NotBeNull();
    }

    [Fact]
    public async Task IsProfileAvailableAsync_throws_when_user_missing()
    {
        Func<Task> act = () => service.IsProfileAvailableAsync(99);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*No profile found*");
    }

    [Fact]
    public async Task IsProfileAvailableAsync_returns_active_account_flag()
    {
        userRepo.Seed(new UserBuilder().WithId(1).WithActiveAccount(false).Build());

        (await service.IsProfileAvailableAsync(1)).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAccountStatusAsync_persists_status_and_touches_LastUpdated()
    {
        userRepo.Seed(new UserBuilder().WithId(1).WithActiveAccount(true).Build());

        await service.UpdateAccountStatusAsync(1, false);

        var user = await userRepo.GetByIdAsync(1);
        user!.ActiveAccount.Should().BeFalse();
        user.LastUpdated.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task UpdateProfilePicturePathAsync_writes_path()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());

        await service.UpdateProfilePicturePathAsync(1, "pic.png");

        (await userRepo.GetByIdAsync(1))!.ProfilePicturePath.Should().Be("pic.png");
    }

    [Fact]
    public async Task UpdateProfilePicturePathAsync_handles_null_path_as_empty()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());

        await service.UpdateProfilePicturePathAsync(1, null!);

        (await userRepo.GetByIdAsync(1))!.ProfilePicturePath.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveProfilePicturePathAsync_clears_path()
    {
        var user = new UserBuilder().WithId(1).Build();
        user.ProfilePicturePath = "old.png";
        userRepo.Seed(user);

        await service.RemoveProfilePicturePathAsync(1);

        (await userRepo.GetByIdAsync(1))!.ProfilePicturePath.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_adds_when_user_missing()
    {
        var user = new UserBuilder().WithId(0).Build();

        await service.SaveAsync(0, user);

        (await userRepo.GetAllAsync()).Should().HaveCount(1);
    }

    [Fact]
    public async Task SaveAsync_updates_when_user_exists()
    {
        userRepo.Seed(new UserBuilder().WithId(1).WithEmail("old@x.com").Build());
        var updated = new UserBuilder().WithId(1).WithEmail("new@x.com").Build();

        await service.SaveAsync(1, updated);

        (await userRepo.GetByIdAsync(1))!.Email.Should().Be("new@x.com");
    }

    [Fact]
    public void GenerateParsedCvText_returns_empty_for_null_user()
    {
        service.GenerateParsedCvText(null!).Should().BeEmpty();
    }

    [Fact]
    public void GenerateParsedCvText_combines_name_university_and_skill_names()
    {
        var user = new UserBuilder().WithId(1).WithName("Ada", "Lovelace").Build();
        user.University = "Cambridge";
        user.Skills = new List<UserSkill>
        {
            new() { SkillId = 1, Skill = new Skill { Name = "C#" } },
            new() { SkillId = 2, Skill = new Skill { Name = "SQL" } },
        };

        var text = service.GenerateParsedCvText(user);

        text.Should().Contain("Ada Lovelace");
        text.Should().Contain("Cambridge");
        text.Should().Contain("C#, SQL");
    }

    [Fact]
    public async Task RecalculateLevelAsync_returns_zero_for_null_user()
    {
        (await service.RecalculateLevelAsync(null!)).Should().Be(0);
    }

    [Fact]
    public async Task RecalculateLevelAsync_sums_xp_from_skill_tests_and_sets_level()
    {
        var user = new UserBuilder().WithId(1).Build();
        userRepo.Seed(user);
        skillTestRepo.Seed(
            new SkillTestBuilder().WithId(1).ForUser(1).WithScore(95).Build(),
            new SkillTestBuilder().WithId(2).ForUser(1).WithScore(75).Build(),
            new SkillTestBuilder().WithId(3).ForUser(1).WithScore(55).Build());

        var totalXp = await service.RecalculateLevelAsync(user);

        var expected = SimpleModelOperations.GoldExperiencePoints
            + SimpleModelOperations.SilverExperiencePoints
            + SimpleModelOperations.BronzeExperiencePoints;
        totalXp.Should().Be(expected);
        user.TotalExperiencePoints.Should().Be(expected);
        user.CurrentLevel.Should().Be(SimpleModelOperations.CalculateLevelNumber(expected));
    }
}
