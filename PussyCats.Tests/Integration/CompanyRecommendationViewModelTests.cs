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
    private readonly FakeUserRepository userRepo = new();
    private readonly FakeJobRepository jobRepo = new();
    private readonly FakeUserSkillRepository userSkillRepo = new();
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
        var companyId = 4;
        var applicantResult = ViewModelTestData.Applicant(matchId: matchId, companyId: companyId, status: MatchStatus.Applied);

        userRepo.Seed(applicantResult.User);
        jobRepo.Seed(applicantResult.Job);
        matchRepo.Seed(applicantResult.Match);

        recommendationService.GetNextApplicant().Returns(applicantResult);
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
        var companyId = 4;
        var applicantResult = ViewModelTestData.Applicant(matchId: matchId, companyId: companyId, status: MatchStatus.Applied);

        userRepo.Seed(applicantResult.User);
        jobRepo.Seed(applicantResult.Job);
        matchRepo.Seed(applicantResult.Match);

        recommendationService.GetNextApplicant().Returns(applicantResult);
        await viewModel.LoadApplicantsAsync();

        await viewModel.SkipApplicantAsync();
        await viewModel.UndoLastActionAsync();

        var persistedMatch = await matchRepo.GetByIdAsync(matchId);
        persistedMatch!.Status.Should().Be(MatchStatus.Applied);
        viewModel.CurrentApplicant.Should().BeSameAs(applicantResult);
    }

    [Fact]
    public async Task LoadApplicantsAsync_CompanyModeActive_CorrectlySyncsStateFromServiceToViewModel()
    {
        var companyId = 4;
        var applicantResult = ViewModelTestData.Applicant(matchId: 7, companyId: companyId, status: MatchStatus.Applied);

        userRepo.Seed(applicantResult.User);
        jobRepo.Seed(applicantResult.Job);
        matchRepo.Seed(applicantResult.Match);

        recommendationService.GetNextApplicant().Returns(applicantResult);

        await viewModel.LoadApplicantsAsync();

        viewModel.CurrentApplicant.Should().BeSameAs(applicantResult);
        viewModel.HasApplicant.Should().BeTrue();
    }

    [Fact]
    public async Task ExpandCardAsync_BreakdownRequested_IntegratesServiceDataIntoViewModelState()
    {
        var companyId = 4;
        var applicantResult = ViewModelTestData.Applicant(matchId: 7, companyId: companyId, status: MatchStatus.Applied);
        var breakdown = new CompatibilityBreakdown { OverallScore = 85 };

        userRepo.Seed(applicantResult.User);
        jobRepo.Seed(applicantResult.Job);
        matchRepo.Seed(applicantResult.Match);

        recommendationService.GetNextApplicant().Returns(applicantResult);
        recommendationService.GetBreakdownAsync(applicantResult, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<CompatibilityBreakdown?>(breakdown));

        await viewModel.LoadApplicantsAsync();
        await viewModel.ExpandCardAsync();

        viewModel.ScoreBreakdown.Should().BeSameAs(breakdown);
        viewModel.IsExpanded.Should().BeTrue();
    }
}