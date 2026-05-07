using FluentAssertions;
using PussyCats.App.Services;

namespace PussyCats.Tests.Services;

public class UserLevelServiceTests
{
    [Fact]
    public void GetExperiencePointsRequiredForLevel_maps_level_to_threshold()
    {
        UserLevelService.GetExperiencePointsRequiredForLevel(1).Should().Be(SimpleModelOperations.Level1ExperiencePoints);
        UserLevelService.GetExperiencePointsRequiredForLevel(2).Should().Be(SimpleModelOperations.Level2ExperiencePoints);
        UserLevelService.GetExperiencePointsRequiredForLevel(3).Should().Be(SimpleModelOperations.Level3ExperiencePoints);
        UserLevelService.GetExperiencePointsRequiredForLevel(4).Should().Be(SimpleModelOperations.Level4ExperiencePoints);
        UserLevelService.GetExperiencePointsRequiredForLevel(5).Should().Be(SimpleModelOperations.Level5ExperiencePoints);
        UserLevelService.GetExperiencePointsRequiredForLevel(99).Should().Be(SimpleModelOperations.Level5ExperiencePoints);
    }

    [Fact]
    public void GetNextLevelExperiencePoints_returns_zero_at_max_level()
    {
        UserLevelService.GetNextLevelExperiencePoints(5).Should().Be(SimpleModelOperations.Level1ExperiencePoints);
        UserLevelService.GetNextLevelExperiencePoints(99).Should().Be(SimpleModelOperations.Level1ExperiencePoints);
    }

    [Fact]
    public void GetNextLevelExperiencePoints_returns_next_threshold()
    {
        UserLevelService.GetNextLevelExperiencePoints(1).Should().Be(SimpleModelOperations.Level2ExperiencePoints);
        UserLevelService.GetNextLevelExperiencePoints(2).Should().Be(SimpleModelOperations.Level3ExperiencePoints);
        UserLevelService.GetNextLevelExperiencePoints(3).Should().Be(SimpleModelOperations.Level4ExperiencePoints);
        UserLevelService.GetNextLevelExperiencePoints(4).Should().Be(SimpleModelOperations.Level5ExperiencePoints);
    }

    [Fact]
    public void GetLevelProgressPercent_returns_zero_at_level_floor()
    {
        UserLevelService.GetLevelProgressPercent(SimpleModelOperations.Level1ExperiencePoints, 1).Should().Be(0);
        UserLevelService.GetLevelProgressPercent(SimpleModelOperations.Level2ExperiencePoints, 2).Should().Be(0);
    }

    [Fact]
    public void GetLevelProgressPercent_returns_full_progress_at_max_level()
    {
        UserLevelService.GetLevelProgressPercent(1500, 5).Should().Be(100);
    }

    [Fact]
    public void GetLevelProgressPercent_returns_proportional_progress_within_level()
    {
        var halfwayInLevel1 = SimpleModelOperations.Level2ExperiencePoints / 2;
        UserLevelService.GetLevelProgressPercent(halfwayInLevel1, 1).Should().Be(50);
    }

    [Fact]
    public void GetLevelProgressPercent_throws_on_negative_xp()
    {
        Action act = () => UserLevelService.GetLevelProgressPercent(-1, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetExperiencePointsToNextLevel_returns_zero_at_max_level()
    {
        UserLevelService.GetExperiencePointsToNextLevel(1500, 5).Should().Be(0);
    }

    [Fact]
    public void GetExperiencePointsToNextLevel_returns_remaining_xp()
    {
        var remaining = UserLevelService.GetExperiencePointsToNextLevel(50, 1);
        remaining.Should().Be(SimpleModelOperations.Level2ExperiencePoints - 50);
    }

    [Fact]
    public void GetExperiencePointsToNextLevel_throws_on_negative_xp()
    {
        Action act = () => UserLevelService.GetExperiencePointsToNextLevel(-1, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CalculateLevelNumber_throws_on_negative_xp()
    {
        Action act = () => UserLevelService.CalculateLevelNumber(-1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CalculateLevelNumber_delegates_to_simple_model_operations()
    {
        UserLevelService.CalculateLevelNumber(0).Should().Be(1);
        UserLevelService.CalculateLevelNumber(800).Should().Be(5);
    }
}
