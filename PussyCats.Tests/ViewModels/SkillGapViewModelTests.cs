using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.DTOs;
using PussyCats.Library.Services.SkillGapService;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Integration;

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

        Assert.True(viewModel.ShowContent);
        Assert.True(viewModel.HasSkillData);
        Assert.False(viewModel.HasSummaryMessage);
        Assert.Single(viewModel.MissingSkills, skill => skill.SkillName == "Docker");
        Assert.Single(viewModel.SkillsToImprove, skill => skill.SkillName == "SQL");
        Assert.True(viewModel.HasMissingSkills);
        Assert.True(viewModel.HasSkillsToImprove);
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

        Assert.True(viewModel.HasSummaryMessage);
        Assert.False(viewModel.HasSkillData);
        Assert.Contains("No rejections", viewModel.SummaryMessage);
    }

    [Fact]
    public async Task LoadDataAsync_ServiceFails_ShowsErrorSummary()
    {
        skillGapService.GetSummaryAsync(9, Arg.Any<CancellationToken>())
            .Returns<Task<SkillGapSummaryModel>>(_ => throw new InvalidOperationException("boom"));

        var viewModel = new SkillGapViewModel(skillGapService, session);

        await viewModel.LoadDataAsync();

        Assert.True(viewModel.ShowContent);
        Assert.True(viewModel.HasSummaryMessage);
        Assert.Contains("Unable to load", viewModel.SummaryMessage);
    }
}
