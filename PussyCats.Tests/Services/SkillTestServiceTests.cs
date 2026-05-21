using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.SkillTests;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

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

        (await skillTestService.CanRetakeTestAsync(skillTestId)).Should().BeTrue();
    }

    [Fact]
    public async Task CanRetakeTestAsync_InvalidSkillTest_ThrowsException()
    {
        int missingTestId = 100;
        Func<Task> act = () => skillTestService.CanRetakeTestAsync(missingTestId);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*No test found*");
    }

    [Fact]
    public void IsRetakeEligible_TestOlderThanThreeMonths_ReturnsTrue()
    {
        DateOnly fourMonthsAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-4));

        var testOldEnough = new SkillTestBuilder()
            .WithAchievedDate(fourMonthsAgo)
            .Build();

        SkillTestService.IsRetakeEligible(testOldEnough).Should().BeTrue();
    }
    [Fact]
    public void IsRetakeEligible_TestYoungerThanThreeMonths_ReturnsFalse()
    {
        DateOnly thirtyDaysAgo = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

        var testTooRecent = new SkillTestBuilder()
            .WithAchievedDate(thirtyDaysAgo)
            .Build();

        SkillTestService.IsRetakeEligible(testTooRecent).Should().BeFalse();
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
        badgeResult.Should().NotBeNull();

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

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*not yet eligible*");
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

        result.Should().Be(expectedFormattedDate);
    }
}
