using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Helpers;
using PussyCats.Tests.Integration;
using PussyCats_App.Services.SkillGapService;

namespace PussyCats.Tests.ViewModels;

public class SkillGapViewModelTests
{
    private readonly ISkillGapService skillGapService = Substitute.For<ISkillGapService>();
    private readonly SessionContext session = new() { UserId = 9 };

    [Fact]
    public async Task LoadDataAsync_GapsExist_PopulatesSkillGapLists()
    {
        skillGapService.GetSummaryAsync(9, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(ViewModelTestData.SkillGapSummary()));
        skillGapService.GetMissingSkillsAsync(9, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<MissingSkillModel>>([new() { SkillName = "Docker", RejectedJobCount = 2 }]));
        skillGapService.GetUnderscoredSkillsAsync(9, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UnderscoredSkillModel>>([new() { SkillName = "SQL", UserScore = 40, AverageRequiredScore = 70 }]));

        var viewModel = new SkillGapViewModel(skillGapService, session);

        await viewModel.LoadDataAsync();

        viewModel.ShowContent.Should().BeTrue();
        viewModel.HasSkillData.Should().BeTrue();
        viewModel.HasSummaryMessage.Should().BeFalse();
        viewModel.MissingSkills.Should().ContainSingle(skill => skill.SkillName == "Docker");
        viewModel.SkillsToImprove.Should().ContainSingle(skill => skill.SkillName == "SQL");
        viewModel.HasMissingSkills.Should().BeTrue();
        viewModel.HasSkillsToImprove.Should().BeTrue();
    }

    [Fact]
    public async Task LoadDataAsync_NoRejections_ShowsGuidanceMessage()
    {
        skillGapService.GetSummaryAsync(9, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(ViewModelTestData.SkillGapSummary(hasRejections: false, hasSkillGaps: false)));
        skillGapService.GetMissingSkillsAsync(9, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<MissingSkillModel>>([]));
        skillGapService.GetUnderscoredSkillsAsync(9, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UnderscoredSkillModel>>([]));

        var viewModel = new SkillGapViewModel(skillGapService, session);

        await viewModel.LoadDataAsync();

        viewModel.HasSummaryMessage.Should().BeTrue();
        viewModel.HasSkillData.Should().BeFalse();
        viewModel.SummaryMessage.Should().Contain("No rejections");
    }

    [Fact]
    public async Task LoadDataAsync_ServiceFails_ShowsErrorSummary()
    {
        skillGapService.GetSummaryAsync(9, Arg.Any<CancellationToken>())
            .Returns<Task<SkillGapSummaryModel>>(_ => throw new InvalidOperationException("boom"));

        var viewModel = new SkillGapViewModel(skillGapService, session);

        await viewModel.LoadDataAsync();

        viewModel.ShowContent.Should().BeTrue();
        viewModel.HasSummaryMessage.Should().BeTrue();
        viewModel.SummaryMessage.Should().Contain("Unable to load");
    }
}