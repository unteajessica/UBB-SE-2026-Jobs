namespace Tests_and_Interviews.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Moq;
    using Xunit;

    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services.Interfaces;
    using Tests_and_Interviews.ViewModels;

    public class LeaderboardViewModelTests
    {
        private readonly Mock<ILeaderboardService> leaderboardServiceMock;

        private readonly LeaderboardViewModel viewModel;

        public LeaderboardViewModelTests()
        {
            this.leaderboardServiceMock = new Mock<ILeaderboardService>();
            this.viewModel = new LeaderboardViewModel(this.leaderboardServiceMock.Object);
        }

        [Fact]
        public async Task LoadAsync_CallsGetFullLeaderboardWithCorrectTestId()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(42))
                .ReturnsAsync(new List<LeaderboardEntry>());

            await this.viewModel.LoadAsync(42);

            this.leaderboardServiceMock.Verify(
                s => s.GetFullLeaderboardAsync(42),
                Times.Once);
        }

        [Fact]
        public async Task LoadAsync_ResetsCurrentPageToOne()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(25));

            await this.viewModel.LoadAsync(1);

            this.viewModel.GoToNextPage();

            await this.viewModel.LoadAsync(1);

            Assert.Equal(1, this.viewModel.CurrentPage);
        }

        [Fact]
        public async Task TotalPages_IsOneWhenEntriesIsEmpty()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<LeaderboardEntry>());

            await this.viewModel.LoadAsync(1);

            Assert.Equal(1, this.viewModel.TotalPages);
        }

        [Fact]
        public async Task TotalPages_IsOneWhenEntriesCountIsExactlyPageSize()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(10));

            await this.viewModel.LoadAsync(1);

            Assert.Equal(1, this.viewModel.TotalPages);
        }

        [Fact]
        public async Task TotalPages_RoundsUpWhenEntriesDoNotFillLastPage()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(11));

            await this.viewModel.LoadAsync(1);

            Assert.Equal(2, this.viewModel.TotalPages);
        }

        [Fact]
        public async Task TotalPages_IsCorrectForMultipleFullPages()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(30));

            await this.viewModel.LoadAsync(1);

            Assert.Equal(3, this.viewModel.TotalPages);
        }

        // --- CanGoPrev ---

        [Fact]
        public async Task CanGoPrev_IsFalseOnFirstPage()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(25));

            await this.viewModel.LoadAsync(1);

            Assert.False(this.viewModel.CanGoPrev);
        }

        [Fact]
        public async Task CanGoPrev_IsTrueAfterMovingToNextPage()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(25));

            await this.viewModel.LoadAsync(1);

            this.viewModel.GoToNextPage();

            Assert.True(this.viewModel.CanGoPrev);
        }

        [Fact]
        public async Task CanGoNext_IsFalseWhenOnlyOnePage()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(5));

            await this.viewModel.LoadAsync(1);

            Assert.False(this.viewModel.CanGoNext);
        }

        [Fact]
        public async Task CanGoNext_IsTrueWhenMorePagesExist()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(25));

            await this.viewModel.LoadAsync(1);

            Assert.True(this.viewModel.CanGoNext);
        }

        [Fact]
        public async Task CanGoNext_IsFalseOnLastPage()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(25));

            await this.viewModel.LoadAsync(1);

            this.viewModel.GoToNextPage();
            this.viewModel.GoToNextPage();
            this.viewModel.GoToNextPage();

            Assert.False(this.viewModel.CanGoNext);
        }

        [Fact]
        public async Task GoToNextPage_IncrementsCurrentPage()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(25));

            await this.viewModel.LoadAsync(1);

            this.viewModel.GoToNextPage();

            Assert.Equal(2, this.viewModel.CurrentPage);
        }

        private static List<LeaderboardEntry> MakeEntries(int count)
        {
            var entries = new List<LeaderboardEntry>();

            for (int i = 1; i <= count; i++)
            {
                entries.Add(new LeaderboardEntry
                {
                    Id = i,
                    TestId = 1,
                    UserId = i,
                    NormalizedScore = 100 - i,
                    RankPosition = i,
                    TieBreakPriority = i,
                });
            }

            return entries;
        }

        [Fact]
        public async Task GoToNextPage_DoesNotExceedTotalPages()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(10));

            await this.viewModel.LoadAsync(1);

            this.viewModel.GoToNextPage();

            Assert.Equal(1, this.viewModel.CurrentPage);
        }

        [Fact]
        public async Task GoToPrevPage_DecrementsCurrentPage()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(25));

            await this.viewModel.LoadAsync(1);

            this.viewModel.GoToNextPage();
            this.viewModel.GoToPrevPage();

            Assert.Equal(1, this.viewModel.CurrentPage);
        }

        [Fact]
        public async Task GoToPrevPage_DoesNotGoBelowOne()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(10));

            await this.viewModel.LoadAsync(1);

            this.viewModel.GoToPrevPage();

            Assert.Equal(1, this.viewModel.CurrentPage);
        }

        [Fact]
        public async Task GetCurrentPageEntries_ReturnsPageSizeEntriesForFullPage()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(25));

            await this.viewModel.LoadAsync(1);

            Assert.Equal(10, this.viewModel.GetCurrentPageEntries().Count);
        }

        [Fact]
        public async Task GetCurrentPageEntries_ReturnsRemainingEntriesOnLastPage()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(13));

            await this.viewModel.LoadAsync(1);

            this.viewModel.GoToNextPage();

            Assert.Equal(3, this.viewModel.GetCurrentPageEntries().Count);
        }

        [Fact]
        public async Task GetCurrentPageEntries_ReturnsEmptyListWhenNoEntries()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<LeaderboardEntry>());

            await this.viewModel.LoadAsync(1);

            Assert.Empty(this.viewModel.GetCurrentPageEntries());
        }

        [Fact]
        public async Task GetCurrentPageEntries_ReturnsCorrectEntriesForSecondPage()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetFullLeaderboardAsync(It.IsAny<int>()))
                .ReturnsAsync(MakeEntries(25));

            await this.viewModel.LoadAsync(1);

            this.viewModel.GoToNextPage();

            Assert.Equal(11, this.viewModel.GetCurrentPageEntries()[0].RankPosition);
        }

        [Fact]
        public async Task GetTopThreeAsync_CallsServiceWithCorrectTestId()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetTopThreeAsync(7))
                .ReturnsAsync(new List<LeaderboardEntry>());

            await this.viewModel.GetTopThreeAsync(7);

            this.leaderboardServiceMock.Verify(
                s => s.GetTopThreeAsync(7),
                Times.Once);
        }

        [Fact]
        public async Task GetTopThreeAsync_ReturnsResultFromService()
        {
            var expected = MakeEntries(3);

            this.leaderboardServiceMock
                .Setup(s => s.GetTopThreeAsync(It.IsAny<int>()))
                .ReturnsAsync(expected);

            var result = await this.viewModel.GetTopThreeAsync(1);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetUserRankingAsync_CallsServiceWithCorrectArguments()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetUserRankingAsync(5, 3))
                .ReturnsAsync((LeaderboardEntry?)null);

            await this.viewModel.GetUserRankingAsync(5, 3);

            this.leaderboardServiceMock.Verify(
                s => s.GetUserRankingAsync(5, 3),
                Times.Once);
        }

        [Fact]
        public async Task GetUserRankingAsync_ReturnsEntryFromService()
        {
            var expected = new LeaderboardEntry
            {
                UserId = 5,
                TestId = 3,
                RankPosition = 2,
            };

            this.leaderboardServiceMock
                .Setup(s => s.GetUserRankingAsync(5, 3))
                .ReturnsAsync(expected);

            var result = await this.viewModel.GetUserRankingAsync(5, 3);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetUserRankingAsync_ReturnsNullWhenUserHasNoEntry()
        {
            this.leaderboardServiceMock
                .Setup(s => s.GetUserRankingAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((LeaderboardEntry?)null);

            var result = await this.viewModel.GetUserRankingAsync(99, 1);

            Assert.Null(result);
        }
    }
}