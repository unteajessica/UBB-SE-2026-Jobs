// <copyright file="AttemptValidationServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TestsAndInterviews.Tests.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Services;
    using Xunit;

    /// <summary>
    /// Contains unit tests for the <see cref="AttemptValidationService"/> class.
    /// </summary>
    public class AttemptValidationServiceTests
    {
        private Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpResponseMessage responseMessage)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage)
                .Verifiable();

            return handlerMock;
        }

        private static TestAttemptDto MakeTestAttemptDto()
        {
            return new TestAttemptDto
            {
                Id = 1,
                TestId = 1,
                ExternalUserId = 1,
            };
        }

        [Fact]
        public async Task CanStartTestAsync_WhenNoExistingAttempt_ReturnsTrue()
        {
            // Arrange - Returning 404 Not Found simulates no existing attempt
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handlerMock = this.CreateMockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
            var validationService = new AttemptValidationService(httpClient);

            // Act
            bool result = await validationService.CanStartTestAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CanStartTestAsync_WhenExistingAttemptFound_ReturnsFalse()
        {
            // Arrange - Returning 200 OK with Dto simulates an existing attempt
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(MakeTestAttemptDto())
            };
            var handlerMock = this.CreateMockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
            var validationService = new AttemptValidationService(httpClient);

            // Act
            bool result = await validationService.CanStartTestAsync(1, 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CheckExistingAttemptsAsync_WhenNoExistingAttempt_DoesNotThrow()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handlerMock = this.CreateMockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
            var validationService = new AttemptValidationService(httpClient);

            // Act
            var exception = await Record.ExceptionAsync(() => validationService.CheckExistingAttemptsAsync(1, 1));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task CheckExistingAttemptsAsync_WhenExistingAttemptFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(MakeTestAttemptDto())
            };
            var handlerMock = this.CreateMockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
            var validationService = new AttemptValidationService(httpClient);

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => validationService.CheckExistingAttemptsAsync(1, 1));
        }

        [Fact]
        public async Task CheckExistingAttemptsAsync_WhenExistingAttemptFound_ExceptionMessageContainsUserId()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(MakeTestAttemptDto())
            };
            var handlerMock = this.CreateMockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
            var validationService = new AttemptValidationService(httpClient);

            // Act
            var exception = await Record.ExceptionAsync(() => validationService.CheckExistingAttemptsAsync(1, 1));

            // Assert
            Assert.Contains("1", exception!.Message);
        }

        [Fact]
        public async Task CanStartTestAsync_WhenCalled_CallsHttpClientOnce()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            var handlerMock = this.CreateMockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
            var validationService = new AttemptValidationService(httpClient);

            // Act
            await validationService.CanStartTestAsync(1, 1);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().EndsWith("testattempts/byuser/1/bytest/1")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public void DefaultConstructor_InitializesHttpClient()
        {
            // Act
            var service = new AttemptValidationService();

            // Assert
            var httpField = typeof(AttemptValidationService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }

        [Fact]
        public void ParametrizedConstructor_WithNull_InitializesWithDefault()
        {
            // Act
            var service = new AttemptValidationService(null!);

            // Assert
            var httpField = typeof(AttemptValidationService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }
    }
}