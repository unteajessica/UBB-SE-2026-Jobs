using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Services;

namespace PussyCats.Tests.Services;

public class UserLevelServiceTests
{
    private const int LevelOne = 1;
    private const int LevelTwo = 2;
    private const int LevelThree = 3;
    private const int LevelFour = 4;
    private const int LevelFive = 5;
    private const int AboveMaxLevel = 99;

    private const int ZeroProgressPercent = 0;
    private const int HalfwayProgressPercent = 50;
    private const int FullProgressPercent = 100;
    private const int NegativeXp = -1;
    private const int MaxLevelSampleXp = 1500;
    private const int SampleXp = 50;

    public static IEnumerable<object[]> ExperienceThresholdCases =>
    [
        new object[] { LevelOne, SimpleModelOperations.Level1ExperiencePoints },
        new object[] { LevelTwo, SimpleModelOperations.Level2ExperiencePoints },
        new object[] { LevelThree, SimpleModelOperations.Level3ExperiencePoints },
        new object[] { LevelFour, SimpleModelOperations.Level4ExperiencePoints },
        new object[] { LevelFive, SimpleModelOperations.Level5ExperiencePoints },
        new object[] { AboveMaxLevel, SimpleModelOperations.Level5ExperiencePoints },
    ];

    public static IEnumerable<object[]> MaxLevelCases =>
    [
        new object[] { LevelFive },
        new object[] { AboveMaxLevel },
    ];

    public static IEnumerable<object[]> NextLevelThresholdCases =>
    [
        new object[] { LevelOne, SimpleModelOperations.Level2ExperiencePoints },
        new object[] { LevelTwo, SimpleModelOperations.Level3ExperiencePoints },
        new object[] { LevelThree, SimpleModelOperations.Level4ExperiencePoints },
        new object[] { LevelFour, SimpleModelOperations.Level5ExperiencePoints },
    ];

    public static IEnumerable<object[]> LevelFloorCases =>
    [
        new object[] { SimpleModelOperations.Level1ExperiencePoints, LevelOne },
        new object[] { SimpleModelOperations.Level2ExperiencePoints, LevelTwo },
    ];

    public static IEnumerable<object[]> LevelMappingCases =>
    [
        new object[] { 0, LevelOne },
        new object[] { 800, LevelFive },
    ];

    [Theory]
    [MemberData(nameof(ExperienceThresholdCases))]
    public void GetExperiencePointsRequiredForLevel_LevelProvided_MapsToThreshold(int level, int expectedXp)
    {
        UserLevelService.GetExperiencePointsRequiredForLevel(level).Should().Be(expectedXp);
    }

    [Theory]
    [MemberData(nameof(MaxLevelCases))]
    public void GetNextLevelExperiencePoints_AtMaxLevel_ReturnsZero(int level)
    {
        UserLevelService.GetNextLevelExperiencePoints(level).Should().Be(SimpleModelOperations.Level1ExperiencePoints);
    }

    [Theory]
    [MemberData(nameof(NextLevelThresholdCases))]
    public void GetNextLevelExperiencePoints_BelowMaxLevel_ReturnsNextThreshold(int level, int expectedXp)
    {
        UserLevelService.GetNextLevelExperiencePoints(level).Should().Be(expectedXp);
    }

    [Theory]
    [MemberData(nameof(LevelFloorCases))]
    public void GetLevelProgressPercent_AtLevelFloor_ReturnsZero(int xp, int level)
    {
        UserLevelService.GetLevelProgressPercent(xp, level).Should().Be(ZeroProgressPercent);
    }

    [Fact]
    public void GetLevelProgressPercent_AtMaxLevel_ReturnsOneHundred()
    {
        UserLevelService.GetLevelProgressPercent(MaxLevelSampleXp, LevelFive).Should().Be(FullProgressPercent);
    }

    [Fact]
    public void GetLevelProgressPercent_HalfwayThroughLevel_ReturnsProportionalProgress()
    {
        var halfwayInLevel1 = SimpleModelOperations.Level2ExperiencePoints / 2;
        UserLevelService.GetLevelProgressPercent(halfwayInLevel1, LevelOne).Should().Be(HalfwayProgressPercent);
    }

    [Fact]
    public void GetLevelProgressPercent_NegativeXpProvided_ThrowsArgumentException()
    {
        Action act = () => UserLevelService.GetLevelProgressPercent(NegativeXp, LevelOne);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetExperiencePointsToNextLevel_AtMaxLevel_ReturnsZero()
    {
        UserLevelService.GetExperiencePointsToNextLevel(MaxLevelSampleXp, LevelFive).Should().Be(ZeroProgressPercent);
    }

    [Fact]
    public void GetExperiencePointsToNextLevel_BelowMaxLevel_ReturnsRemainingXp()
    {
        var remaining = UserLevelService.GetExperiencePointsToNextLevel(SampleXp, LevelOne);
        remaining.Should().Be(SimpleModelOperations.Level2ExperiencePoints - SampleXp);
    }

    [Fact]
    public void GetExperiencePointsToNextLevel_NegativeXpProvided_ThrowsArgumentException()
    {
        Action act = () => UserLevelService.GetExperiencePointsToNextLevel(NegativeXp, LevelOne);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CalculateLevelNumber_NegativeXpProvided_ThrowsArgumentException()
    {
        Action act = () => UserLevelService.CalculateLevelNumber(NegativeXp);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(LevelMappingCases))]
    public void CalculateLevelNumber_ValidXpProvided_MapsXpToLevel(int xp, int expectedLevel)
    {
        UserLevelService.CalculateLevelNumber(xp).Should().Be(expectedLevel);
    }
}
