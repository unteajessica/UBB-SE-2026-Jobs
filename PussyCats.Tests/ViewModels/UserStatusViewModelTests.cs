using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.Tests.ViewModels;

public class UserStatusViewModelTests
{
    private readonly IUserStatusService statusService = Substitute.For<IUserStatusService>();
    private readonly ISkillGapService skillGapService = Substitute.For<ISkillGapService>();
    private readonly SessionContext session = new() { UserId = 7 };

    [Fact]
    public async Task LoadMatchesAsync_populates_applications_and_skill_gap_sidebar()
    {
        statusService.GetApplicationsForUserAsync(7, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ApplicationCardModel>>(
            [
                ViewModelTestData.Application(1, MatchStatus.Applied),
                ViewModelTestData.Application(2, MatchStatus.Rejected),
            ]));
        skillGapService.GetSummaryAsync(7, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(ViewModelTestData.SkillGapSummary()));
        skillGapService.GetMissingSkillsAsync(7, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<MissingSkillModel>>([new() { SkillName = "Docker", RejectedJobCount = 2 }]));
        skillGapService.GetUnderscoredSkillsAsync(7, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UnderscoredSkillModel>>([new() { SkillName = "SQL", UserScore = 40, AverageRequiredScore = 70 }]));
        var viewModel = new UserStatusViewModel(statusService, skillGapService, session);

        await viewModel.LoadMatchesAsync();

        viewModel.AppliedJobs.Should().HaveCount(2);
        viewModel.FilteredJobs.Should().HaveCount(2);
        viewModel.ShowCards.Should().BeTrue();
        viewModel.ShowSkillData.Should().BeTrue();
        viewModel.HasUnderscoredSkills.Should().BeTrue();
        viewModel.HasSidebarMissingSkills.Should().BeTrue();
        viewModel.SkillGapSummaryText.Should().Contain("1 missing skills");
    }

    [Fact]
    public async Task ApplyFilter_filters_by_status_and_sets_empty_message()
    {
        statusService.GetApplicationsForUserAsync(7, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ApplicationCardModel>>(
            [
                ViewModelTestData.Application(1, MatchStatus.Applied),
                ViewModelTestData.Application(2, MatchStatus.Accepted),
            ]));
        skillGapService.GetSummaryAsync(7, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(ViewModelTestData.SkillGapSummary(hasRejections: false, hasSkillGaps: false)));
        skillGapService.GetMissingSkillsAsync(7, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<MissingSkillModel>>([]));
        skillGapService.GetUnderscoredSkillsAsync(7, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UnderscoredSkillModel>>([]));
        var viewModel = new UserStatusViewModel(statusService, skillGapService, session);
        await viewModel.LoadMatchesAsync();

        viewModel.ApplyFilter("Rejected");

        viewModel.FilteredJobs.Should().BeEmpty();
        viewModel.IsEmpty.Should().BeTrue();
        viewModel.EmptyMessage.Should().Be("No applications match this filter.");
        viewModel.ShowGoToRecommendations.Should().BeFalse();
    }

    [Fact]
    public async Task LoadMatchesAsync_sets_error_state_when_service_throws()
    {
        statusService.GetApplicationsForUserAsync(7, Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<ApplicationCardModel>>>(_ => throw new InvalidOperationException("boom"));
        var viewModel = new UserStatusViewModel(statusService, skillGapService, session);

        await viewModel.LoadMatchesAsync();

        viewModel.HasError.Should().BeTrue();
        viewModel.ShowCards.Should().BeFalse();
        viewModel.IsLoading.Should().BeFalse();
    }
}
