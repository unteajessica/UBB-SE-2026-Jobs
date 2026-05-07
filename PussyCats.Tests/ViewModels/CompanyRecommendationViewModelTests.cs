using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.Tests.ViewModels;

public class CompanyRecommendationViewModelTests
{
    private readonly ICompanyRecommendationService recommendationService = Substitute.For<ICompanyRecommendationService>();
    private readonly IMatchService matchService = Substitute.For<IMatchService>();
    private readonly SessionContext session = new() { CompanyId = 4, Mode = AppMode.Company };

    [Fact]
    public async Task LoadApplicantsAsync_loads_current_applicant()
    {
        var applicant = ViewModelTestData.Applicant(companyId: 4);
        recommendationService.GetNextApplicant().Returns(applicant);
        var viewModel = new CompanyRecommendationViewModel(recommendationService, matchService, session);

        await viewModel.LoadApplicantsAsync();

        await recommendationService.Received(1).LoadApplicantsAsync(4, Arg.Any<CancellationToken>());
        viewModel.CurrentApplicant.Should().BeSameAs(applicant);
        viewModel.HasApplicant.Should().BeTrue();
        viewModel.StatusMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadApplicantsAsync_reports_when_company_mode_inactive()
    {
        session.Mode = AppMode.Candidate;
        var viewModel = new CompanyRecommendationViewModel(recommendationService, matchService, session);

        await viewModel.LoadApplicantsAsync();

        viewModel.CurrentApplicant.Should().BeNull();
        viewModel.StatusMessage.Should().Be("Company mode is not active.");
    }

    [Fact]
    public async Task AdvanceApplicantAsync_advances_current_applicant_and_stores_undo()
    {
        var applicant = ViewModelTestData.Applicant(matchId: 8, companyId: 4);
        recommendationService.GetNextApplicant().Returns(applicant, (UserApplicationResult?)null);
        matchService.GetByIdAsync(8, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Match?>(new Match { MatchId = 8, Status = MatchStatus.Applied }));
        var viewModel = new CompanyRecommendationViewModel(recommendationService, matchService, session);
        await viewModel.LoadApplicantsAsync();

        await viewModel.AdvanceApplicantAsync();

        await matchService.Received(1).AdvanceAsync(8, Arg.Any<CancellationToken>());
        recommendationService.Received(1).MoveToNext();
        viewModel.CurrentApplicant.Should().BeNull();
        viewModel.StatusMessage.Should().Be("No more applicants to review.");
        viewModel.CanUndo.Should().BeTrue();
    }

    [Fact]
    public async Task UndoLastActionAsync_reverts_match_and_restores_applicant()
    {
        var applicant = ViewModelTestData.Applicant(matchId: 8, companyId: 4);
        recommendationService.GetNextApplicant().Returns(applicant, (UserApplicationResult?)null);
        matchService.GetByIdAsync(8, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Match?>(new Match { MatchId = 8, Status = MatchStatus.Applied }));
        var viewModel = new CompanyRecommendationViewModel(recommendationService, matchService, session);
        await viewModel.LoadApplicantsAsync();
        await viewModel.SkipApplicantAsync();

        await viewModel.UndoLastActionAsync();

        await matchService.Received(1).RevertToAppliedAsync(8, Arg.Any<CancellationToken>());
        recommendationService.Received(1).MoveToPrevious();
        viewModel.CurrentApplicant.Should().BeSameAs(applicant);
        viewModel.CanUndo.Should().BeFalse();
    }

    [Fact]
    public async Task ExpandCardAsync_loads_breakdown_and_marks_card_expanded()
    {
        var applicant = ViewModelTestData.Applicant(companyId: 4);
        var breakdown = new CompatibilityBreakdown { OverallScore = 81 };
        recommendationService.GetNextApplicant().Returns(applicant);
        recommendationService.GetBreakdownAsync(applicant, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<CompatibilityBreakdown?>(breakdown));
        var viewModel = new CompanyRecommendationViewModel(recommendationService, matchService, session);
        await viewModel.LoadApplicantsAsync();

        await viewModel.ExpandCardAsync();

        viewModel.ScoreBreakdown.Should().BeSameAs(breakdown);
        viewModel.IsExpanded.Should().BeTrue();
    }
}
