using System;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Services;
using Xunit;

namespace TestsAndInterviews.Tests.Services
{
    public class SessionServiceTests
    {
        [Fact]
        public void Constructor_SetsLoggedInUser()
        {
            // Arrange
            var company = new Company { CompanyId = 1, Name = "Test Company" };

            // Act
            var service = new SessionService(company);

            // Assert
            Assert.Same(company, service.LoggedInUser);
        }
    }
}
