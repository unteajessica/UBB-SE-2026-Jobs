using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;

namespace PussyCats.Tests.Integration;

public class CompanyRecommendationViewModelTests
{
    private readonly FakeMatchRepository matchRepo = new();
    private readonly FakeJobRepository jobRepo = new();
    private readonly ICompanyRecommendationService recommendationService = Substitute.For<ICompanyRecommendationService>();
    private readonly SessionContext session = new() { CompanyId = 4, Mode = AppMode.Company };

    private readonly MatchService matchService;
    private readonly CompanyRecommendationViewModel viewModel;

    public CompanyRecommendationViewModelTests()
    {
        matchService = new MatchService(matchRepo, new JobService(jobRepo));
        viewModel = new CompanyRecommendationViewModel(recommendationService, matchService, session);
    }

    [Fact]
    public async Task AdvanceApplicantAsync_CommandExecuted_PersistsStatusChangeInRepository()
    {
        var matchId = 8;
        var applicant = ViewModelTestData.Applicant(matchId: matchId, companyId: 4);
        matchRepo.Seed(new MatchBuilder().WithId(matchId).WithStatus(MatchStatus.Applied).Build());

        recommendationService.GetNextApplicant().Returns(applicant);
        await viewModel.LoadApplicantsAsync();

        await viewModel.AdvanceApplicantAsync();

        var persistedMatch = await matchRepo.GetByIdAsync(matchId);
        persistedMatch!.Status.Should().Be(MatchStatus.Advanced);
        viewModel.CanUndo.Should().BeTrue();
    }

    [Fact]
    public async Task UndoLastActionAsync_AfterSkip_RestoresOriginalStatusInRepository()
    {
        var matchId = 10;
        var applicant = ViewModelTestData.Applicant(matchId: matchId, companyId: 4);
        matchRepo.Seed(new MatchBuilder().WithId(matchId).WithStatus(MatchStatus.Applied).Build());

        recommendationService.GetNextApplicant().Returns(applicant);
        await viewModel.LoadApplicantsAsync();

        await viewModel.SkipApplicantAsync();
        await viewModel.UndoLastActionAsync();

        var persistedMatch = await matchRepo.GetByIdAsync(matchId);
        persistedMatch!.Status.Should().Be(MatchStatus.Applied);
        viewModel.CurrentApplicant.Should().BeSameAs(applicant);
    }

    [Fact]
    public async Task LoadApplicantsAsync_CompanyModeActive_CorrectlySyncsStateFromServiceToViewModel()
    {
        var applicant = ViewModelTestData.Applicant(companyId: 4);
        recommendationService.GetNextApplicant().Returns(applicant);

        await viewModel.LoadApplicantsAsync();

        viewModel.CurrentApplicant.Should().BeSameAs(applicant);
        viewModel.HasApplicant.Should().BeTrue();
    }

    [Fact]
    public async Task ExpandCardAsync_BreakdownRequested_IntegratesServiceDataIntoViewModelState()
    {
        var applicant = ViewModelTestData.Applicant(companyId: 4);
        var breakdown = new CompatibilityBreakdown { OverallScore = 85 };
        recommendationService.GetNextApplicant().Returns(applicant);
        recommendationService.GetBreakdownAsync(applicant, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<CompatibilityBreakdown?>(breakdown));

        await viewModel.LoadApplicantsAsync();
        await viewModel.ExpandCardAsync();

        viewModel.ScoreBreakdown.Should().BeSameAs(breakdown);
        viewModel.IsExpanded.Should().BeTrue();
    }
}