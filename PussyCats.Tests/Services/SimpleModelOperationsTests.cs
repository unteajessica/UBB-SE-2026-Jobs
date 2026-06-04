using PussyCats.Library.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services;

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
    public void GetExperiencePoints_ScoreProvided_ReturnsTierXpForScore(int score, int expectedXp)
    {
        var skillTest = new SkillTest { Score = score };

        Assert.Equal(expectedXp, SimpleModelOperations.GetExperiencePoints(skillTest));
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
    public void CalculateLevelNumber_ExperiencePointsProvided_MapsXpToLevel(int xp, int expectedLevel)
    {
        Assert.Equal(expectedLevel, SimpleModelOperations.CalculateLevelNumber(xp));
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
    public void AssignTier_ScoreProvided_ClassifiesScoreIntoBadge(float score, BadgeTier expectedTier, int expectedXp)
    {
        var badge = SimpleModelOperations.AssignTier(score);

        Assert.Equal(expectedTier, badge.Tier);
        Assert.Equal(expectedXp, badge.ExperiencePointsValue);
        Assert.NotEmpty(badge.IconPath);
    }

    [Fact]
    public void TierThresholds_ConstantsDefined_AreStrictlyOrdered()
    {
        Assert.True(SimpleModelOperations.GoldScoreThreshold > SimpleModelOperations.SilverScoreThreshold);
        Assert.True(SimpleModelOperations.SilverScoreThreshold > SimpleModelOperations.BronzeScoreThreshold);
        Assert.True(SimpleModelOperations.BronzeScoreThreshold > 0);
    }

    [Fact]
    public void LevelThresholds_ConstantsDefined_AreStrictlyOrdered()
    {
        Assert.Equal(0, SimpleModelOperations.Level1ExperiencePoints);
        Assert.True(SimpleModelOperations.Level2ExperiencePoints > SimpleModelOperations.Level1ExperiencePoints);
        Assert.True(SimpleModelOperations.Level3ExperiencePoints > SimpleModelOperations.Level2ExperiencePoints);
        Assert.True(SimpleModelOperations.Level4ExperiencePoints > SimpleModelOperations.Level3ExperiencePoints);
        Assert.True(SimpleModelOperations.Level5ExperiencePoints > SimpleModelOperations.Level4ExperiencePoints);
    }
}