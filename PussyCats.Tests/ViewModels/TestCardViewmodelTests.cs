// <copyright file="TestCardViewModelTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PussyCats.Tests.ViewModels
{
    using Xunit;

    using Tests_and_Interviews.ViewModels;

    public class TestCardViewModelTests
    {
        [Fact]
        public void IsSelected_WhenSetToTrue_ReturnsTrue()
        {
            var testCard = new TestCardViewModel();

            testCard.IsSelected = true;

            Assert.True(testCard.IsSelected);
        }

        [Fact]
        public void IsSelected_WhenSetToFalse_ReturnsFalse()
        {
            var testCard = new TestCardViewModel();

            testCard.IsSelected = true;
            testCard.IsSelected = false;

            Assert.False(testCard.IsSelected);
        }

        [Fact]
        public void IsHovered_WhenSetToTrue_ReturnsTrue()
        {
            var testCard = new TestCardViewModel();

            testCard.IsHovered = true;

            Assert.True(testCard.IsHovered);
        }

        [Fact]
        public void IsHovered_WhenSetToFalse_ReturnsFalse()
        {
            var testCard = new TestCardViewModel();

            testCard.IsHovered = true;
            testCard.IsHovered = false;

            Assert.False(testCard.IsHovered);
        }

        [Fact]
        public void OnPropertyChanged_WhenNoListenersAttached_DoesNotThrow()
        {
            var testCard = new TestCardViewModel();

            var exception = Record.Exception(() => testCard.IsSelected = true);

            Assert.Null(exception);
        }

        [Fact]
        public void PropertyChanged_WhenIsSelectedChanges_FiresEvent()
        {
            var testCard = new TestCardViewModel();

            var fired = false;

            testCard.PropertyChanged += (sender, eventArgs) =>
            {
                fired = true;
            };

            testCard.IsSelected = true;

            Assert.True(fired);
        }

        [Fact]
        public void PropertyChanged_WhenIsHoveredChanges_FiresEvent()
        {
            var testCard = new TestCardViewModel();

            var fired = false;

            testCard.PropertyChanged += (sender, eventArgs) =>
            {
                fired = true;
            };

            testCard.IsHovered = true;

            Assert.True(fired);
        }
    }
}