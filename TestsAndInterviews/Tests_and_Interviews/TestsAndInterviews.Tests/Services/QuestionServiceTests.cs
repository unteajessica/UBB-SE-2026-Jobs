using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Tests_and_Interviews.Dtos;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;
using Xunit;

namespace TestsAndInterviews.Tests.Services
{
    public class QuestionServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly QuestionService _service;

        public QuestionServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/api/")
            };
            _service = new QuestionService(_httpClient);
        }

        [Fact]
        public async Task FindByTestIdAsync_WhenSuccessful_ReturnsQuestions()
        {
            // Arrange
            var testId = 1;
            var dtos = new List<QuestionDto>
            {
                new QuestionDto { Id = 1, QuestionText = "Q1" },
                new QuestionDto { Id = 2, QuestionText = "Q2" }
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().EndsWith($"/questions/bytest/{testId}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(dtos)
                });

            // Act
            var result = await _service.FindByTestIdAsync(testId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Q1", result[0].QuestionText);
        }

        [Fact]
        public async Task FindByTestIdAsync_WhenNotFound_ReturnsEmptyList()
        {
            // Arrange
            var testId = 999;
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().EndsWith($"/questions/bytest/{testId}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var result = await _service.FindByTestIdAsync(testId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FindByTestIdAsync_WhenApiFails_ThrowsHttpRequestException()
        {
            // Arrange
            var testId = 1;
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _service.FindByTestIdAsync(testId));
        }

        [Fact]
        public void DefaultConstructor_InitializesHttpClient()
        {
            // Act
            var service = new QuestionService();

            // Assert
            var httpField = typeof(QuestionService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }
    }
}
