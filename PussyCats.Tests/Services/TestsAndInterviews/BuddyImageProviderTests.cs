// <copyright file="BuddyImageProviderTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Services;
using Xunit;
namespace PussyCats.Tests.Services.TestsAndInterviews
{
    public class BuddyImageProviderTests
    {
        [Theory]
        [InlineData(0, "ms-appx:///Assets/AvatarFemale.png")]
        [InlineData(1, "ms-appx:///Assets/AvatarMale.png")]
        [InlineData(2, "ms-appx:///Assets/AvatarFemale.png")]
        [InlineData(-1, "ms-appx:///Assets/AvatarFemale.png")]
        public void GetImagePathById_ReturnsCorrectPath(int id, string expectedPath)
        {
            // Act
            var result = BuddyImageProvider.GetImagePathById(id);

            // Assert
            Assert.Equal(expectedPath, result);
        }
    }
}
