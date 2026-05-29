using FluentAssertions;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Fakes;
using PussyCats.Tests.Helpers;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.Users;
using PussyCats.Library.Services.CompanyStatusService;
using PussyCats.Library.Services.UserSkillService;

namespace PussyCats.Tests.Integration;

public class CompanyStatusViewModelTests
{
    private readonly FakeMatchRepository matchRepository = new();
    private readonly FakeUserRepository userRepository = new();
    private readonly FakeJobRepository jobRepository = new();
    private readonly FakeUserSkillRepository userSkillRepository = new();

    private readonly CompanyStatusViewModel viewModel;

    public CompanyStatusViewModelTests()
    {
        var session = new SessionContext { CompanyId = 4, Mode = AppMode.Company };

        var userService = new UserService(userRepository);
        var jobService = new JobService(jobRepository);
        var userSkillService = new UserSkillService(userSkillRepository);
        var matchService = new MatchService(matchRepository, jobService, new UserService(userRepository));

        var companyStatusService = new CompanyStatusService(
            matchService,
            userService,
            jobService,
            userSkillService);

        viewModel = new CompanyStatusViewModel(companyStatusService, matchService, session);
    }

    [Fact]
    public async Task LoadApplicationsAsync_DataExistsInRepo_PopulatesApplicationsAndPageMessage()
    {
        var companyId = 4;
        var applicant = ViewModelTestData.Applicant(matchId: 7, companyId: companyId, status: MatchStatus.Advanced);

        userRepository.Seed(applicant.User);
        jobRepository.Seed(applicant.Job);
        matchRepository.Seed(applicant.Match);

        await viewModel.LoadApplicationsAsync();

        viewModel.Applications.Should().HaveCount(1);
        viewModel.PageMessage.Should().Be("1 applicant(s) are Accepted, Rejected, or In Review.");
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadEvaluationAsync_MatchInRepo_SetsSelectedApplicantAndEvaluationState()
    {
        var matchId = 12;
        var companyId = 4;
        var applicant = ViewModelTestData.Applicant(matchId: matchId, companyId: companyId, status: MatchStatus.Accepted);

        userRepository.Seed(applicant.User);
        jobRepository.Seed(applicant.Job);
        matchRepository.Seed(applicant.Match);

        var loaded = await viewModel.LoadEvaluationAsync(matchId);

        loaded.Should().BeTrue();
        viewModel.SelectedApplicant.Should().NotBeNull();
        viewModel.SelectedApplicant!.Match.MatchId.Should().Be(matchId);
        viewModel.SelectedDecision.Should().Be(MatchStatus.Accepted);
        viewModel.FeedbackMessage.Should().Be("Good fit.");
    }

    [Fact]
    public async Task LoadEvaluationAsync_FinalDecisionMatch_DisablesDecisionEditing()
    {
        var matchId = 13;
        var companyId = 4;
        var applicant = ViewModelTestData.Applicant(matchId: matchId, companyId: companyId, status: MatchStatus.Rejected);

        userRepository.Seed(applicant.User);
        jobRepository.Seed(applicant.Job);
        matchRepository.Seed(applicant.Match);

        var loaded = await viewModel.LoadEvaluationAsync(matchId);

        loaded.Should().BeTrue();
        viewModel.SelectedDecision.Should().Be(MatchStatus.Rejected);
        viewModel.CanEditDecision.Should().BeFalse();
    }

    [Fact]
    public async Task LoadEvaluationAsync_AcceptedMatch_DisablesDecisionEditing()
    {
        var matchId = 14;
        var companyId = 4;
        var applicant = ViewModelTestData.Applicant(matchId: matchId, companyId: companyId, status: MatchStatus.Accepted);

        userRepository.Seed(applicant.User);
        jobRepository.Seed(applicant.Job);
        matchRepository.Seed(applicant.Match);

        var loaded = await viewModel.LoadEvaluationAsync(matchId);

        loaded.Should().BeTrue();
        viewModel.SelectedDecision.Should().Be(MatchStatus.Accepted);
        viewModel.CanEditDecision.Should().BeFalse();
    }

    [Fact]
    public void ValidateAll_EmptyState_ReportsMissingDecisionAndFeedbackErrors()
    {
        var isValid = viewModel.ValidateAll();

        isValid.Should().BeFalse();
        viewModel.HasValidationErrors.Should().BeTrue();
        viewModel.ValidationErrorDecision.Should().Contain("Select an applicant");
        viewModel.ValidationErrorFeedback.Should().Contain("required");
    }

    [Fact]
    public async Task SubmitDecisionAsync_ValidSubmission_PersistsDecisionInRepositoryAndRefreshesList()
    {
        var matchId = 15;
        var companyId = 4;
        var applicant = ViewModelTestData.Applicant(matchId: matchId, companyId: companyId, status: MatchStatus.Advanced);

        userRepository.Seed(applicant.User);
        jobRepository.Seed(applicant.Job);
        matchRepository.Seed(applicant.Match);

        await viewModel.LoadEvaluationAsync(matchId);
        viewModel.SelectedDecision = MatchStatus.Rejected;
        viewModel.FeedbackMessage = "Not enough experience.";

        var saved = await viewModel.SubmitDecisionAsync();

        saved.Should().BeTrue();
        var persistedMatch = await matchRepository.GetByIdAsync(matchId);
        persistedMatch!.Status.Should().Be(MatchStatus.Rejected);
        persistedMatch.FeedbackMessage.Should().Be("Not enough experience.");
        viewModel.SelectedApplicant.Should().BeNull();
    }
}
