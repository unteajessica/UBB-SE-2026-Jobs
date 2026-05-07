using FluentAssertions;
using PussyCats.App.Services;

namespace PussyCats.Tests.Services;

public class UserLevelServiceTests
{
    [Theory]
    [InlineData(1, SimpleModelOperations.Level1ExperiencePoints)]
    [InlineData(2, SimpleModelOperations.Level2ExperiencePoints)]
    [InlineData(3, SimpleModelOperations.Level3ExperiencePoints)]
    [InlineData(4, SimpleModelOperations.Level4ExperiencePoints)]
    [InlineData(5, SimpleModelOperations.Level5ExperiencePoints)]
    [InlineData(99, SimpleModelOperations.Level5ExperiencePoints)]
    public void GetExperiencePointsRequiredForLevel_LevelProvided_MapsToThreshold(int level, int expectedXp)
    {
        UserLevelService.GetExperiencePointsRequiredForLevel(level).Should().Be(expectedXp);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(99)]
    public void GetNextLevelExperiencePoints_AtMaxLevel_ReturnsZero(int level)
    {
        UserLevelService.GetNextLevelExperiencePoints(level).Should().Be(SimpleModelOperations.Level1ExperiencePoints);
    }

    [Theory]
    [InlineData(1, SimpleModelOperations.Level2ExperiencePoints)]
    [InlineData(2, SimpleModelOperations.Level3ExperiencePoints)]
    [InlineData(3, SimpleModelOperations.Level4ExperiencePoints)]
    [InlineData(4, SimpleModelOperations.Level5ExperiencePoints)]
    public void GetNextLevelExperiencePoints_BelowMaxLevel_ReturnsNextThreshold(int level, int expectedXp)
    {
        UserLevelService.GetNextLevelExperiencePoints(level).Should().Be(expectedXp);
    }

    [Theory]
    [InlineData(SimpleModelOperations.Level1ExperiencePoints, 1)]
    [InlineData(SimpleModelOperations.Level2ExperiencePoints, 2)]
    public void GetLevelProgressPercent_AtLevelFloor_ReturnsZero(int xp, int level)
    {
        UserLevelService.GetLevelProgressPercent(xp, level).Should().Be(0);
    }

    [Fact]
    public void GetLevelProgressPercent_AtMaxLevel_ReturnsOneHundred()
    {
        UserLevelService.GetLevelProgressPercent(1500, 5).Should().Be(100);
    }

    [Fact]
    public void GetLevelProgressPercent_HalfwayThroughLevel_ReturnsProportionalProgress()
    {
        var halfwayInLevel1 = SimpleModelOperations.Level2ExperiencePoints / 2;
        UserLevelService.GetLevelProgressPercent(halfwayInLevel1, 1).Should().Be(50);
    }

    [Fact]
    public void GetLevelProgressPercent_NegativeXpProvided_ThrowsArgumentException()
    {
        Action act = () => UserLevelService.GetLevelProgressPercent(-1, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetExperiencePointsToNextLevel_AtMaxLevel_ReturnsZero()
    {
        UserLevelService.GetExperiencePointsToNextLevel(1500, 5).Should().Be(0);
    }

    [Fact]
    public void GetExperiencePointsToNextLevel_BelowMaxLevel_ReturnsRemainingXp()
    {
        var remaining = UserLevelService.GetExperiencePointsToNextLevel(50, 1);
        remaining.Should().Be(SimpleModelOperations.Level2ExperiencePoints - 50);
    }

    [Fact]
    public void GetExperiencePointsToNextLevel_NegativeXpProvided_ThrowsArgumentException()
    {
        Action act = () => UserLevelService.GetExperiencePointsToNextLevel(-1, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CalculateLevelNumber_NegativeXpProvided_ThrowsArgumentException()
    {
        Action act = () => UserLevelService.CalculateLevelNumber(-1);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(800, 5)]
    public void CalculateLevelNumber_ValidXpProvided_MapsXpToLevel(int xp, int expectedLevel)
    {
        UserLevelService.CalculateLevelNumber(xp).Should().Be(expectedLevel);
    }
}