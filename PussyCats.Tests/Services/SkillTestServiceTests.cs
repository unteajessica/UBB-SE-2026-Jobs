using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.SkillTests;
using PussyCats.Tests.Fakes;


namespace PussyCats.Tests.Services;

public class SkillTestServiceTests
{
    private readonly FakeSkillTestRepository skillTestRepository = new();
    private readonly SkillTestService skillTestService;


    public SkillTestServiceTests()
    {
        skillTestService = new SkillTestService(skillTestRepository);
    }


    [Fact]
    public async Task CanRetakeTestAsync_ValidSkillTest_ReturnsTrue()
    {
        DateOnly fourMonthsAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4));
        int skillTestId = 1;

        skillTestRepository.Seed(new SkillTestBuilder()
            .WithId(skillTestId)
            .WithAchievedDate(fourMonthsAgo)
            .Build());

        Assert.True(await skillTestService.CanRetakeTestAsync(skillTestId));
    }

    [Fact]
    public async Task CanRetakeTestAsync_InvalidSkillTest_ThrowsException()
    {
        int missingTestId = 100;
        Func<Task> act = () => skillTestService.CanRetakeTestAsync(missingTestId);

        var ex = await Assert.ThrowsAsync<Exception>(act);
        Assert.Contains("No test found", ex.Message);
    }

    [Fact]
    public void IsRetakeEligible_TestOlderThanThreeMonths_ReturnsTrue()
    {
        DateOnly fourMonthsAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4));

        var testOldEnough = new SkillTestBuilder()
            .WithAchievedDate(fourMonthsAgo)
            .Build();

        Assert.True(SkillTestService.IsRetakeEligible(testOldEnough));
    }
    [Fact]
    public void IsRetakeEligible_TestYoungerThanThreeMonths_ReturnsFalse()
    {
        DateOnly thirtyDaysAgo = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

        var testTooRecent = new SkillTestBuilder()
            .WithAchievedDate(thirtyDaysAgo)
            .Build();

        Assert.False(SkillTestService.IsRetakeEligible(testTooRecent));
    }

    [Fact]
    public async Task SubmitRetakeAsync_EligibleTest_ReturnsBadge()
    {
        int skillTestId = 1;
        int initialScore = 40;
        int newScore = 95;
        DateOnly sixMonthsAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6));
        skillTestRepository.Seed(new SkillTestBuilder()
            .WithId(skillTestId)
            .WithScore(initialScore)
            .WithAchievedDate(sixMonthsAgo)
            .Build());

        var badgeResult = await skillTestService.SubmitRetakeAsync(skillTestId, newScore);
        Assert.NotNull(badgeResult);

    }


    [Fact]
    public async Task SubmitRetakeAsync_NotYetEligible_ThrowsException()
    {
        int skillTestId = 1;
        int newScore = 95;
        DateOnly tenDaysAgo = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        skillTestRepository.Seed(new SkillTestBuilder()
            .WithId(skillTestId)
            .WithAchievedDate(tenDaysAgo)
            .Build());

        Func<Task> act = () => skillTestService.SubmitRetakeAsync(skillTestId, newScore);

        var ex = await Assert.ThrowsAsync<Exception>(act);
        Assert.Contains("not yet eligible", ex.Message);
    }


    [Fact]
    public void AchievedDateFormatted_ReturnsDateIn_ddMMyyyy_Format()
    {
        string expectedFormattedDate = "12.05.2025";
        int year = 2025;
        int month = 5;
        int day = 12;

        DateOnly achievedDate = new DateOnly(year, month, day);
        var skillTest = new SkillTestBuilder()
            .WithAchievedDate(achievedDate)
            .Build();

        var result = SkillTestService.AchievedDateFormatted(skillTest);

        Assert.Equal(expectedFormattedDate, result);
    }
}
