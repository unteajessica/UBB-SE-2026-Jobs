using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Integration;
using PussyCats.Library.Services.UserRecommendationService;

namespace PussyCats.Tests.ViewModels;

public class UserRecommendationViewModelTests
{
    private readonly IUserRecommendationService service = Substitute.For<IUserRecommendationService>();
    private readonly SessionContext session = new() { UserId = 5, Mode = AppMode.Candidate };

    [Fact]
    public async Task LoadRecommendationsAsync_CardExists_SetsCurrentCardFromService()
    {
        var card = ViewModelTestData.JobCard();
        service.GetNextCardAsync(5, Arg.Any<UserMatchmakingFilters>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JobRecommendationResult?>(card));
        var viewModel = new UserRecommendationViewModel(service, session);

        await viewModel.LoadRecommendationsAsync();

        Assert.Same(card, viewModel.CurrentJob);
        Assert.True(viewModel.HasCard);
        Assert.False(viewModel.ShowEmptyDeck);
    }

    [Fact]
    public async Task LoadRecommendationsAsync_SessionModeIsCompany_ReportsMissingCandidateSessionError()
    {
        session.Mode = AppMode.Company;
        var viewModel = new UserRecommendationViewModel(service, session);
        var errors = new List<string>();
        viewModel.ErrorOccurred += errors.Add;

        await viewModel.LoadRecommendationsAsync();

        Assert.Null(viewModel.CurrentJob);
        Assert.Contains("Candidate session", viewModel.ErrorMessage);
        Assert.Contains("Candidate session", Assert.Single(errors));
    }

    [Fact]
    public async Task LikeAsync_ValidCard_AppliesLikeAdvancesToNextAndEnablesUndo()
    {
        var first = ViewModelTestData.JobCard(jobId: 10, recommendationId: 100);
        var second = ViewModelTestData.JobCard(jobId: 11, recommendationId: 101);

        service.GetNextCardAsync(5, Arg.Any<UserMatchmakingFilters>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JobRecommendationResult?>(first), Task.FromResult<JobRecommendationResult?>(second));
        service.ApplyLikeAsync(5, first, Arg.Any<CancellationToken>()).Returns(Task.FromResult(44));

        var viewModel = new UserRecommendationViewModel(service, session);
        await viewModel.LoadRecommendationsAsync();

        await viewModel.LikeAsync();

        await service.Received(1).ApplyLikeAsync(5, first, Arg.Any<CancellationToken>());
        Assert.Same(second, viewModel.CurrentJob);
        Assert.True(viewModel.CanUndo);
    }

    [Fact]
    public async Task UndoAsync_AfterLike_RestoresPreviousCardAndDisablesUndo()
    {
        var first = ViewModelTestData.JobCard(jobId: 10, recommendationId: 100);
        var second = ViewModelTestData.JobCard(jobId: 11, recommendationId: 101);

        service.GetNextCardAsync(5, Arg.Any<UserMatchmakingFilters>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JobRecommendationResult?>(first), Task.FromResult<JobRecommendationResult?>(second));
        service.ApplyLikeAsync(5, first, Arg.Any<CancellationToken>()).Returns(Task.FromResult(44));

        var viewModel = new UserRecommendationViewModel(service, session);
        await viewModel.LoadRecommendationsAsync();
        await viewModel.LikeAsync();

        await viewModel.UndoAsync();

        await service.Received(1).UndoLikeAsync(44, first.DisplayRecommendationId, Arg.Any<CancellationToken>());
        Assert.Same(first, viewModel.CurrentJob);
        Assert.False(viewModel.CanUndo);
    }

    [Fact]
    public async Task ApplyFiltersAsync_FiltersSet_PassesSelectedValuesToService()
    {
        UserMatchmakingFilters? capturedFilters = null;
        service.GetNextCardAsync(5, Arg.Do<UserMatchmakingFilters>(filters => capturedFilters = filters), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JobRecommendationResult?>(null));
        service.RecalculateTopCardIgnoringCooldownAsync(5, Arg.Any<UserMatchmakingFilters>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JobRecommendationResult?>(null));

        var viewModel = new UserRecommendationViewModel(service, session);

        // Setup draft selections
        viewModel.DraftEmploymentSelections.First(filterCheckItem => filterCheckItem.Label == "Full-time").IsChecked = true;
        viewModel.DraftExperienceSelections.First(filterCheckItem => filterCheckItem.Label == "Entry").IsChecked = true;
        viewModel.SetSkillFilterOptions([new SkillFilterItem(3, "C#") { IsChecked = true }]);
        viewModel.DraftLocation = " Cluj ";

        await viewModel.ApplyFiltersAsync();

        Assert.NotNull(capturedFilters);
        Assert.Contains("Full-time", capturedFilters!.EmploymentTypes);
        Assert.Contains("Entry", capturedFilters.ExperienceLevels);
        Assert.Contains(3, capturedFilters.SkillIds);
        Assert.Equal("Cluj", capturedFilters.LocationSubstring);
        Assert.False(viewModel.IsFilterOpen);
    }
}