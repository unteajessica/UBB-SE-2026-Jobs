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
    public async Task IsProfileAvailableAsync_UserIsMissing_ThrowsException()
    {
        Func<Task> act = () => service.IsProfileAvailableAsync(99);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*No profile found*");
    }

    [Fact]
    public async Task IsProfileAvailableAsync_ProfileExists_ReturnsActiveAccountFlag()
    {
        userRepo.Seed(new UserBuilder().WithId(1).WithActiveAccount(false).Build());

        (await service.IsProfileAvailableAsync(1)).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAccountStatusAsync_StatusChanged_PersistsStatusAndTouchesLastUpdated()
    {
        userRepo.Seed(new UserBuilder().WithId(1).WithActiveAccount(true).Build());

        await service.UpdateAccountStatusAsync(1, false);

        var user = await userRepo.GetByIdAsync(1);
        user!.ActiveAccount.Should().BeFalse();
        user.LastUpdated.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task UpdateProfilePicturePathAsync_PathProvided_WritesPathToUser()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());

        await service.UpdateProfilePicturePathAsync(1, "pic.png");

        (await userRepo.GetByIdAsync(1))!.ProfilePicturePath.Should().Be("pic.png");
    }

    [Fact]
    public async Task UpdateProfilePicturePathAsync_PathIsNull_HandlesAsEmptyString()
    {
        userRepo.Seed(new UserBuilder().WithId(1).Build());

        await service.UpdateProfilePicturePathAsync(1, null!);

        (await userRepo.GetByIdAsync(1))!.ProfilePicturePath.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveProfilePicturePathAsync_UserHasPicture_ClearsPath()
    {
        var user = new UserBuilder().WithId(1).Build();
        user.ProfilePicturePath = "old.png";
        userRepo.Seed(user);

        await service.RemoveProfilePicturePathAsync(1);

        (await userRepo.GetByIdAsync(1))!.ProfilePicturePath.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_UserIsMissing_AddsNewUser()
    {
        var user = new UserBuilder().WithId(0).Build();

        await service.SaveAsync(0, user);

        (await userRepo.GetAllAsync()).Should().HaveCount(1);
    }

    [Fact]
    public async Task SaveAsync_UserExists_UpdatesExistingUser()
    {
        userRepo.Seed(new UserBuilder().WithId(1).WithEmail("old@x.com").Build());
        var updated = new UserBuilder().WithId(1).WithEmail("new@x.com").Build();

        await service.SaveAsync(1, updated);

        (await userRepo.GetByIdAsync(1))!.Email.Should().Be("new@x.com");
    }

    [Fact]
    public void GenerateParsedCvText_UserIsNull_ReturnsEmptyString()
    {
        service.GenerateParsedCvText(null!).Should().BeEmpty();
    }

    [Fact]
    public void GenerateParsedCvText_ValidUserProvided_CombinesNameUniversityAndSkillNames()
    {
        var user = new UserBuilder().WithId(1).WithName("Ada", "Lovelace").Build();
        user.University = "Cambridge";
        user.Skills = new List<UserSkill>
        {
            new() { Skill = new Skill { SkillId = 1, Name = "C#" } },
            new() { Skill = new Skill { SkillId = 2, Name = "SQL" } },
        };

        var text = service.GenerateParsedCvText(user);

        text.Should().Contain("Ada Lovelace");
        text.Should().Contain("Cambridge");
        text.Should().Contain("C#, SQL");
    }

    [Fact]
    public async Task RecalculateLevelAsync_UserIsNull_ReturnsZero()
    {
        (await service.RecalculateLevelAsync(null!)).Should().Be(0);
    }

    [Fact]
    public async Task RecalculateLevelAsync_UserHasSkillTests_SumsXpFromTestsAndSetsLevel()
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
