using FluentAssertions;
using NSubstitute;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using PussyCats.Tests.Integration;

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

        viewModel.CurrentJob.Should().BeSameAs(card);
        viewModel.HasCard.Should().BeTrue();
        viewModel.ShowEmptyDeck.Should().BeFalse();
    }

    [Fact]
    public async Task LoadRecommendationsAsync_SessionModeIsCompany_ReportsMissingCandidateSessionError()
    {
        session.Mode = AppMode.Company;
        var viewModel = new UserRecommendationViewModel(service, session);
        var errors = new List<string>();
        viewModel.ErrorOccurred += errors.Add;

        await viewModel.LoadRecommendationsAsync();

        viewModel.CurrentJob.Should().BeNull();
        viewModel.ErrorMessage.Should().Contain("Candidate session");
        errors.Should().ContainSingle().Which.Should().Contain("Candidate session");
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
        viewModel.CurrentJob.Should().BeSameAs(second);
        viewModel.CanUndo.Should().BeTrue();
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
        viewModel.CurrentJob.Should().BeSameAs(first);
        viewModel.CanUndo.Should().BeFalse();
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
        viewModel.DraftEmploymentSelections.First(x => x.Label == "Full-time").IsChecked = true;
        viewModel.DraftExperienceSelections.First(x => x.Label == "Entry").IsChecked = true;
        viewModel.SetSkillFilterOptions([new SkillFilterItem(3, "C#") { IsChecked = true }]);
        viewModel.DraftLocation = " Cluj ";

        await viewModel.ApplyFiltersAsync();

        capturedFilters.Should().NotBeNull();
        capturedFilters!.EmploymentTypes.Should().Contain("Full-time");
        capturedFilters.ExperienceLevels.Should().Contain("Entry");
        capturedFilters.SkillIds.Should().Contain(3);
        capturedFilters.LocationSubstring.Should().Be("Cluj");
        viewModel.IsFilterOpen.Should().BeFalse();
    }
}