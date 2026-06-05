// <copyright file="MainTestViewModelTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PussyCats.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.UI.Xaml;
    using Moq;
    using Xunit;

    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services.Interfaces;
    using Tests_and_Interviews.ViewModels;

    public class MainTestViewModelTests
    {
        private readonly Mock<ITestService> mockTestService;

        public MainTestViewModelTests()
        {
            this.mockTestService = new Mock<ITestService>();

            this.mockTestService
                .Setup(testService => testService.FindTestsByCategoryAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Test>());
        }

        [Fact]
        public async Task LoadTestsAsync_WhenTestsExist_PopulatesTests()
        {
            var tests = new List<Test>
            {
                new Test
                {
                    Id = 1,
                    Title = "C# Basics",
                    Category = "Programming",
                    Questions = new List<Question>
                    {
                        new Question { QuestionTypeString = "SINGLE_CHOICE" },
                    },
                },
            };

            this.mockTestService
                .Setup(testService => testService.FindTestsByCategoryAsync("Programming"))
                .ReturnsAsync(tests);

            var viewModel = this.CreateViewModel();

            await viewModel.LoadTestsAsync();

            Assert.Single(viewModel.Tests);
            Assert.Equal("C# Basics", viewModel.Tests[0].Title);
            Assert.Equal("SINGLE/CHOICE", viewModel.Tests[0].QuestionTypeLabel);
        }

        [Fact]
        public async Task LoadTestsAsync_WhenTestHasNoQuestions_SetsTypeLabelToMixed()
        {
            var tests = new List<Test>
            {
                new Test
                {
                    Id = 1,
                    Title = "Empty Test",
                    Category = "Programming",
                    Questions = new List<Question>(),
                },
            };

            this.mockTestService
                .Setup(testService => testService.FindTestsByCategoryAsync("Programming"))
                .ReturnsAsync(tests);

            var viewModel = this.CreateViewModel();

            await viewModel.LoadTestsAsync();

            Assert.Equal("MIXED", viewModel.Tests[0].QuestionTypeLabel);
        }

        [Fact]
        public async Task LoadTestsAsync_WhenNoTestsExist_LeavesTestsEmpty()
        {
            var viewModel = this.CreateViewModel();

            await viewModel.LoadTestsAsync();

            Assert.Empty(viewModel.Tests);
        }

        [Fact]
        public async Task LoadTestsAsync_SetsIsLoadingFalseWhenComplete()
        {
            var viewModel = this.CreateViewModel();

            await viewModel.LoadTestsAsync();

            Assert.False(viewModel.IsLoading);
        }

        [Fact]
        public async Task LoadTestsAsync_WhenQuestionsIsNull_SetsTypeLabelToMixed()
        {
            var tests = new List<Test>
            {
                new Test
                {
                    Id = 1,
                    Title = "Null Questions Test",
                    Category = "Programming",
                    Questions = null,
                },
            };

            this.mockTestService
                .Setup(testService => testService.FindTestsByCategoryAsync("Programming"))
                .ReturnsAsync(tests);

            var viewModel = this.CreateViewModel();

            await viewModel.LoadTestsAsync();

            Assert.Equal("MIXED", viewModel.Tests[0].QuestionTypeLabel);
        }

        [Fact]
        public async Task NoTestsVisible_WhenNoTestsAndNotLoading_ReturnsVisible()
        {
            var viewModel = this.CreateViewModel();

            await viewModel.LoadTestsAsync();

            Assert.Equal(Visibility.Visible, viewModel.NoTestsVisible);
        }

        [Fact]
        public async Task NoTestsVisible_WhenTestsExist_ReturnsCollapsed()
        {
            var tests = new List<Test>
            {
                new Test
                {
                    Id = 1,
                    Title = "Test",
                    Category = "Programming",
                    Questions = new List<Question>(),
                },
            };

            this.mockTestService
                .Setup(testService => testService.FindTestsByCategoryAsync("Programming"))
                .ReturnsAsync(tests);

            var viewModel = this.CreateViewModel();

            await viewModel.LoadTestsAsync();

            Assert.Equal(Visibility.Collapsed, viewModel.NoTestsVisible);
        }

        [Fact]
        public void NoTestsVisible_WhenIsLoadingTrue_ReturnsCollapsed()
        {
            var viewModel = this.CreateViewModel();

            viewModel.IsLoading = true;

            Assert.Equal(Visibility.Collapsed, viewModel.NoTestsVisible);
        }

        [Fact]
        public void SelectedTest_WhenSet_MarksNewTestAsSelected()
        {
            var viewModel = this.CreateViewModel();

            var testCard = new TestCardViewModel { TestId = 1 };

            viewModel.SelectedTest = testCard;

            Assert.True(testCard.IsSelected);
        }

        [Fact]
        public void SelectedTest_WhenChanged_DeselectsPreviousTest()
        {
            var viewModel = this.CreateViewModel();

            var firstCard = new TestCardViewModel { TestId = 1 };
            var secondCard = new TestCardViewModel { TestId = 2 };

            viewModel.SelectedTest = firstCard;
            viewModel.SelectedTest = secondCard;

            Assert.False(firstCard.IsSelected);
            Assert.True(secondCard.IsSelected);
        }

        [Fact]
        public void SelectedTest_WhenSetToNull_DoesNotThrow()
        {
            var viewModel = this.CreateViewModel();

            var exception = Record.Exception(() => viewModel.SelectedTest = null);

            Assert.Null(exception);
        }

        [Fact]
        public void SelectedTest_WhenGet_ReturnsCurrentValue()
        {
            var viewModel = this.CreateViewModel();

            var testCard = new TestCardViewModel { TestId = 1 };

            viewModel.SelectedTest = testCard;

            Assert.Equal(testCard, viewModel.SelectedTest);
        }

        [Fact]
        public void OnPropertyChanged_WhenNoListenersAttached_DoesNotThrow()
        {
            var viewModel = this.CreateViewModel();

            var exception = Record.Exception(() => viewModel.IsLoading = true);

            Assert.Null(exception);
        }

        private MainTestViewModel CreateViewModel()
        {
            return new MainTestViewModel(this.mockTestService.Object);
        }
    }
}