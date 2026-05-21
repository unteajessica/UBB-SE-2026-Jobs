using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Library.Services;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Services;

public class UserProfileServiceTests
{
    private const int MissingUserId = 99;
    private const int ExistingUserId = 1;
    private const int NewUserId = 0;
    private const int OneMinute = 1;

    private const int SkillTestIdOne = 1;
    private const int SkillTestIdTwo = 2;
    private const int SkillTestIdThree = 3;
    private const int SkillIdOne = 1;
    private const int SkillIdTwo = 2;
    private const int GoldScore = 95;
    private const int SilverScore = 75;
    private const int BronzeScore = 55;

    private const string OldEmail = "old@x.com";
    private const string NewEmail = "new@x.com";
    private const string ProfilePicturePath = "pic.png";
    private const string OldProfilePicturePath = "old.png";
    private const string UniversityName = "Cambridge";
    private const string FirstName = "Ada";
    private const string LastName = "Lovelace";
    private const string PrimarySkillName = "C#";
    private const string SecondarySkillName = "SQL";

    private readonly FakeUserRepository userRepository = new();
    private readonly FakeSkillTestRepository skillTestRepository = new();
    private readonly UserProfileService service;

    public UserProfileServiceTests()
    {
        service = new UserProfileService(userRepository, skillTestRepository);
    }


    [Fact]
    public async Task IsProfileAvailableAsync_UserIsMissing_ThrowsException()
    {
        Func<Task> act = () => service.IsProfileAvailableAsync(MissingUserId);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*No profile found*");
    }

    [Fact]
    public async Task IsProfileAvailableAsync_ProfileExists_ReturnsActiveAccountFlag()
    {
        userRepository.Seed(new UserBuilder().WithId(ExistingUserId).WithActiveAccount(false).Build());

        (await service.IsProfileAvailableAsync(ExistingUserId)).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAccountStatusAsync_StatusChanged_PersistsStatusAndTouchesLastUpdated()
    {
        userRepository.Seed(new UserBuilder().WithId(ExistingUserId).WithActiveAccount(true).Build());

        await service.UpdateAccountStatusAsync(ExistingUserId, false);

        var user = await userRepository.GetByIdAsync(ExistingUserId);
        user!.ActiveAccount.Should().BeFalse();
        user.LastUpdated.Should().BeAfter(DateTime.UtcNow.AddMinutes(-OneMinute));
    }

    [Fact]
    public async Task UpdateProfilePicturePathAsync_PathProvided_WritesPathToUser()
    {
        userRepository.Seed(new UserBuilder().WithId(ExistingUserId).Build());

        await service.UpdateProfilePicturePathAsync(ExistingUserId, ProfilePicturePath);

        (await userRepository.GetByIdAsync(ExistingUserId))!.ProfilePicturePath.Should().Be(ProfilePicturePath);
    }

    [Fact]
    public async Task UpdateProfilePicturePathAsync_PathIsNull_HandlesAsEmptyString()
    {
        userRepository.Seed(new UserBuilder().WithId(ExistingUserId).Build());

        await service.UpdateProfilePicturePathAsync(ExistingUserId, null!);

        (await userRepository.GetByIdAsync(ExistingUserId))!.ProfilePicturePath.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveProfilePicturePathAsync_UserHasPicture_ClearsPath()
    {
        var user = new UserBuilder().WithId(ExistingUserId).Build();
        user.ProfilePicturePath = OldProfilePicturePath;
        userRepository.Seed(user);

        await service.RemoveProfilePicturePathAsync(ExistingUserId);

        (await userRepository.GetByIdAsync(ExistingUserId))!.ProfilePicturePath.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_UserIsMissing_AddsNewUser()
    {
        var user = new UserBuilder().WithId(NewUserId).Build();

        await service.SaveAsync(NewUserId, user);

        (await userRepository.GetAllAsync()).Should().HaveCount(1);
    }

    [Fact]
    public async Task SaveAsync_UserExists_UpdatesExistingUser()
    {
        userRepository.Seed(new UserBuilder().WithId(ExistingUserId).WithEmail(OldEmail).Build());
        var updated = new UserBuilder().WithId(ExistingUserId).WithEmail(NewEmail).Build();

        await service.SaveAsync(ExistingUserId, updated);

        (await userRepository.GetByIdAsync(ExistingUserId))!.Email.Should().Be(NewEmail);
    }

    [Fact]
    public void GenerateParsedCvText_UserIsNull_ReturnsEmptyString()
    {
        PussyCats.Library.Services.Helpers.GenerateParsedCvText(null!).Should().BeEmpty();
    }

    [Fact]
    public void GenerateParsedCvText_ValidUserProvided_CombinesNameUniversityAndSkillNames()
    {
        var user = new UserBuilder().WithId(ExistingUserId).WithName(FirstName, LastName).Build();
        user.University = UniversityName;
        user.Skills = new List<UserSkill>
        {
            new() { Skill = new Skill { SkillId = SkillIdOne, Name = PrimarySkillName } },
            new() { Skill = new Skill { SkillId = SkillIdTwo, Name = SecondarySkillName } },
        };

        var text = PussyCats.Library.Services.Helpers.GenerateParsedCvText(user);

        text.Should().Contain($"{FirstName} {LastName}");
        text.Should().Contain(UniversityName);
        text.Should().Contain($"{PrimarySkillName}, {SecondarySkillName}");
    }

    [Fact]
    public async Task RecalculateLevelAsync_UserIsNull_ReturnsZero()
    {
        (await service.RecalculateLevelAsync(null!)).Should().Be(0);
    }

    [Fact]
    public async Task RecalculateLevelAsync_UserHasSkillTests_SumsXpFromTestsAndSetsLevel()
    {
        var user = new UserBuilder().WithId(ExistingUserId).Build();
        userRepository.Seed(user);
        skillTestRepository.Seed(
            new SkillTestBuilder().WithId(SkillTestIdOne).ForUser(ExistingUserId).WithScore(GoldScore).Build(),
            new SkillTestBuilder().WithId(SkillTestIdTwo).ForUser(ExistingUserId).WithScore(SilverScore).Build(),
            new SkillTestBuilder().WithId(SkillTestIdThree).ForUser(ExistingUserId).WithScore(BronzeScore).Build());

        var totalXp = await service.RecalculateLevelAsync(user);

        var expected = SimpleModelOperations.GoldExperiencePoints
            + SimpleModelOperations.SilverExperiencePoints
            + SimpleModelOperations.BronzeExperiencePoints;
        totalXp.Should().Be(expected);
        user.TotalExperiencePoints.Should().Be(expected);
        user.CurrentLevel.Should().Be(SimpleModelOperations.CalculateLevelNumber(expected));
    }
}
