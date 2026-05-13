using FluentAssertions;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Companies;
using PussyCats.Library.Repositories.Jobs;
using PussyCats.Library.Repositories.Matches;
using PussyCats.Library.Repositories.Skills;
using PussyCats.Tests.Fakes;
using PussyCats_App.Services.CompanyService;
using PussyCats_App.Services.JobService;
using PussyCats_App.Services.JobSkillService;
using PussyCats_App.Services.SkillGapService;
using PussyCats_App.Services.UserSkillService;
using PussyCats_App.Services.UserStatusService;

namespace PussyCats.Tests.Integration;

public class UserStatusViewModelTests
{
    private readonly IMatchRepository matchRepo = new FakeMatchRepository();
    private readonly IJobRepository jobRepo = new FakeJobRepository();
    private readonly IUserSkillRepository userSkillRepo = new FakeUserSkillRepository();
    private readonly IJobSkillRepository jobSkillRepo = new FakeJobSkillRepository();
    private readonly ICompanyRepository companyRepo = new FakeCompanyRepository();
    
    private readonly SessionContext session = new() { UserId = 12 };

    private readonly UserStatusViewModel viewModel;

    public UserStatusViewModelTests()
    {
        var jobService = new JobService(jobRepo);
        var userSkillService = new UserSkillService(userSkillRepo);
        var jobSkillService = new JobSkillService(jobSkillRepo);
        var companyService = new CompanyService(companyRepo);

        var statusService = new UserStatusService(matchRepo, jobService,companyService, userSkillService, jobSkillService);
        var skillGapService = new SkillGapService(matchRepo, jobSkillService,userSkillService);

        viewModel = new UserStatusViewModel(statusService, skillGapService, session);
    }

    [Fact]
    public async Task LoadMatchesAsync_MatchesExistInRepo_PopulatesApplicationsAndSkillGapSidebar()
    {
        var applicant = ViewModelTestData.Applicant(matchId: 1, status: MatchStatus.Applied);
        
        await jobRepo.AddAsync(applicant.Job);
        await matchRepo.AddAsync(applicant.Match);

        await viewModel.LoadMatchesAsync();

        viewModel.AppliedJobs.Should().NotBeEmpty();
        viewModel.ShowCards.Should().BeTrue();
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task ApplyFilter_NoMatchesForStatus_SetsEmptyStateAndMessage()
    {
        var applicant = ViewModelTestData.Applicant(matchId: 1, status: MatchStatus.Applied);
        await jobRepo.AddAsync(applicant.Job);
        await matchRepo.AddAsync(applicant.Match);
        await viewModel.LoadMatchesAsync();

        viewModel.ApplyFilter("Accepted");

        viewModel.FilteredJobs.Should().BeEmpty();
        viewModel.IsEmpty.Should().BeTrue();
        viewModel.EmptyMessage.Should().Be("No applications match this filter.");
    }
}