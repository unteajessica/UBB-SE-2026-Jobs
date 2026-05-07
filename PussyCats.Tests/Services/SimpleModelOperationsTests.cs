using FluentAssertions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.Tests.Services;

public class SimpleModelOperationsTests
{
    [Theory]
    [InlineData(100, SimpleModelOperations.GoldExperiencePoints)]
    [InlineData((int)SimpleModelOperations.GoldScoreThreshold, SimpleModelOperations.GoldExperiencePoints)]
    [InlineData(89, SimpleModelOperations.SilverExperiencePoints)]
    [InlineData((int)SimpleModelOperations.SilverScoreThreshold, SimpleModelOperations.SilverExperiencePoints)]
    [InlineData(69, SimpleModelOperations.BronzeExperiencePoints)]
    [InlineData((int)SimpleModelOperations.BronzeScoreThreshold, SimpleModelOperations.BronzeExperiencePoints)]
    [InlineData(49, SimpleModelOperations.ParticipantExperiencePoints)]
    [InlineData(0, SimpleModelOperations.ParticipantExperiencePoints)]
    public void GetExperiencePoints_returns_tier_xp_for_score(int score, int expectedXp)
    {
        var skillTest = new SkillTest { Score = score };

        SimpleModelOperations.GetExperiencePoints(skillTest).Should().Be(expectedXp);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(99, 1)]
    [InlineData(100, 2)]
    [InlineData(249, 2)]
    [InlineData(250, 3)]
    [InlineData(499, 3)]
    [InlineData(500, 4)]
    [InlineData(799, 4)]
    [InlineData(800, 5)]
    [InlineData(10_000, 5)]
    public void CalculateLevelNumber_maps_xp_to_level(int xp, int expectedLevel)
    {
        SimpleModelOperations.CalculateLevelNumber(xp).Should().Be(expectedLevel);
    }

    [Theory]
    [InlineData(95f, BadgeTier.Gold, SimpleModelOperations.GoldExperiencePoints)]
    [InlineData(90f, BadgeTier.Gold, SimpleModelOperations.GoldExperiencePoints)]
    [InlineData(75f, BadgeTier.Silver, SimpleModelOperations.SilverExperiencePoints)]
    [InlineData(70f, BadgeTier.Silver, SimpleModelOperations.SilverExperiencePoints)]
    [InlineData(55f, BadgeTier.Bronze, SimpleModelOperations.BronzeExperiencePoints)]
    [InlineData(50f, BadgeTier.Bronze, SimpleModelOperations.BronzeExperiencePoints)]
    [InlineData(49f, BadgeTier.Participant, SimpleModelOperations.ParticipantExperiencePoints)]
    [InlineData(0f, BadgeTier.Participant, SimpleModelOperations.ParticipantExperiencePoints)]
    public void AssignTier_classifies_score_into_badge(float score, BadgeTier expectedTier, int expectedXp)
    {
        var badge = SimpleModelOperations.AssignTier(score);

        badge.Tier.Should().Be(expectedTier);
        badge.ExperiencePointsValue.Should().Be(expectedXp);
        badge.IconPath.Should().NotBeEmpty();
    }

    [Fact]
    public void Tier_thresholds_are_strictly_ordered()
    {
        SimpleModelOperations.GoldScoreThreshold.Should().BeGreaterThan(SimpleModelOperations.SilverScoreThreshold);
        SimpleModelOperations.SilverScoreThreshold.Should().BeGreaterThan(SimpleModelOperations.BronzeScoreThreshold);
        SimpleModelOperations.BronzeScoreThreshold.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Level_thresholds_are_strictly_ordered()
    {
        SimpleModelOperations.Level1ExperiencePoints.Should().Be(0);
        SimpleModelOperations.Level2ExperiencePoints.Should().BeGreaterThan(SimpleModelOperations.Level1ExperiencePoints);
        SimpleModelOperations.Level3ExperiencePoints.Should().BeGreaterThan(SimpleModelOperations.Level2ExperiencePoints);
        SimpleModelOperations.Level4ExperiencePoints.Should().BeGreaterThan(SimpleModelOperations.Level3ExperiencePoints);
        SimpleModelOperations.Level5ExperiencePoints.Should().BeGreaterThan(SimpleModelOperations.Level4ExperiencePoints);
    }
}
