using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;

namespace PussyCats.Tests.ViewModels;

public class CompanyStatusViewModelTests
{
    private readonly ICompanyStatusService companyStatusService = Substitute.For<ICompanyStatusService>();
    private readonly IMatchService matchService = Substitute.For<IMatchService>();
    private readonly SessionContext session = new() { CompanyId = 4, Mode = AppMode.Company };

    [Fact]
    public async Task LoadApplicationsAsync_populates_applications_and_page_message()
    {
        companyStatusService.GetApplicantsForCompanyAsync(4, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UserApplicationResult>>([ViewModelTestData.Applicant(companyId: 4)]));
        var viewModel = new CompanyStatusViewModel(companyStatusService, matchService, session);

        await viewModel.LoadApplicationsAsync();

        viewModel.Applications.Should().HaveCount(1);
        viewModel.PageMessage.Should().Be("1 applicant(s) are Accepted, Rejected, or In Review.");
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadEvaluationAsync_sets_selected_applicant_and_latest_test_stub()
    {
        var applicant = ViewModelTestData.Applicant(matchId: 12, companyId: 4, status: MatchStatus.Accepted);
        companyStatusService.GetApplicantByMatchIdAsync(4, 12, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserApplicationResult?>(applicant));
        var viewModel = new CompanyStatusViewModel(companyStatusService, matchService, session);

        var loaded = await viewModel.LoadEvaluationAsync(12);

        loaded.Should().BeTrue();
        viewModel.SelectedApplicant.Should().BeSameAs(applicant);
        viewModel.SelectedDecision.Should().Be(MatchStatus.Accepted);
        viewModel.FeedbackMessage.Should().Be("Good fit.");
        viewModel.LastTestResult.Should().NotBeNull();
    }

    [Fact]
    public void ValidateAll_reports_missing_decision_and_feedback()
    {
        var viewModel = new CompanyStatusViewModel(companyStatusService, matchService, session);

        var isValid = viewModel.ValidateAll();

        isValid.Should().BeFalse();
        viewModel.HasValidationErrors.Should().BeTrue();
        viewModel.ValidationErrorDecision.Should().Contain("Select an applicant");
        viewModel.ValidationErrorFeedback.Should().Contain("required");
    }

    [Fact]
    public async Task SubmitDecisionAsync_saves_decision_and_refreshes_applications()
    {
        var applicant = ViewModelTestData.Applicant(matchId: 12, companyId: 4, status: MatchStatus.Accepted);
        companyStatusService.GetApplicantByMatchIdAsync(4, 12, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserApplicationResult?>(applicant));
        companyStatusService.GetApplicantsForCompanyAsync(4, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UserApplicationResult>>([]));
        var viewModel = new CompanyStatusViewModel(companyStatusService, matchService, session);
        await viewModel.LoadEvaluationAsync(12);
        viewModel.SelectedDecision = MatchStatus.Rejected;
        viewModel.FeedbackMessage = "Not enough SQL experience.";

        var saved = await viewModel.SubmitDecisionAsync();

        saved.Should().BeTrue();
        await matchService.Received(1).SubmitDecisionAsync(
            12,
            MatchStatus.Rejected,
            "Not enough SQL experience.",
            Arg.Any<CancellationToken>());
        viewModel.SelectedApplicant.Should().BeNull();
    }
}
