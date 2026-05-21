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
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.JobSkills;
using PussyCats_App.Services.SkillGapService;
using PussyCats_App.Services.UserSkillService;
using PussyCats_App.Services.UserStatusService;
using PussyCats.Library.Services.CompanyService;

namespace PussyCats.Tests.Integration;

public class UserStatusViewModelTests
{
    private readonly IMatchRepository matchRepository = new FakeMatchRepository();
    private readonly IJobRepository jobRepository = new FakeJobRepository();
    private readonly IUserSkillRepository userSkillRepository = new FakeUserSkillRepository();
    private readonly IJobSkillRepository jobSkillRepository = new FakeJobSkillRepository();
    private readonly ICompanyRepository companyRepository = new FakeCompanyRepository();
    
    private readonly SessionContext session = new() { UserId = 12 };

    private readonly UserStatusViewModel viewModel;

    public UserStatusViewModelTests()
    {
        var jobService = new JobService(jobRepository);
        var userSkillService = new UserSkillService(userSkillRepository);
        var jobSkillService = new JobSkillService(jobSkillRepository);
        var companyService = new CompanyService(companyRepository);

        var statusService = new UserStatusService(matchRepository, jobService,companyService, userSkillService, jobSkillService);
        var skillGapService = new SkillGapService(matchRepository, jobSkillService,userSkillService);

        viewModel = new UserStatusViewModel(statusService, skillGapService, session);
    }

    [Fact]
    public async Task LoadMatchesAsync_MatchesExistInRepo_PopulatesApplicationsAndSkillGapSidebar()
    {
        var applicant = ViewModelTestData.Applicant(matchId: 1, status: MatchStatus.Applied);
        
        await jobRepository.AddAsync(applicant.Job);
        await matchRepository.AddAsync(applicant.Match);

        await viewModel.LoadMatchesAsync();

        viewModel.AppliedJobs.Should().NotBeEmpty();
        viewModel.ShowCards.Should().BeTrue();
        viewModel.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task ApplyFilter_NoMatchesForStatus_SetsEmptyStateAndMessage()
    {
        var applicant = ViewModelTestData.Applicant(matchId: 1, status: MatchStatus.Applied);
        await jobRepository.AddAsync(applicant.Job);
        await matchRepository.AddAsync(applicant.Match);
        await viewModel.LoadMatchesAsync();

        viewModel.ApplyFilter("Accepted");

        viewModel.FilteredJobs.Should().BeEmpty();
        viewModel.IsEmpty.Should().BeTrue();
        viewModel.EmptyMessage.Should().Be("No applications match this filter.");
    }
}