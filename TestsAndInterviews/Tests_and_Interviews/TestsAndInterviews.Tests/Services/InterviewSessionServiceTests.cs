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
    public class InterviewSessionServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly InterviewSessionService _service;

        public InterviewSessionServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/api/")
            };
            _service = new InterviewSessionService(_httpClient);
        }

        [Fact]
        public void Constructor_WithoutArgs_DoesNotThrow()
        {
            // Act
            var service = new InterviewSessionService();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_DoesNotThrow()
        {
            // Act
            var service = new InterviewSessionService(null!);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task GetSessionAsync_WhenSuccessful_ReturnsSession()
        {
            // Arrange
            var sessionId = 1;
            var dto = new InterviewSessionDto { Id = sessionId, PositionId = 10, Status = "Scheduled" };

            SetupMockResponse(HttpMethod.Get, $"interviewsessions/{sessionId}", HttpStatusCode.OK, dto);

            // Act
            var result = await _service.GetSessionAsync(sessionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sessionId, result.Id);
            Assert.Equal(10, result.PositionId);
        }

        [Fact]
        public async Task StartSessionAsync_WhenSessionExists_ReturnsSessionAndQuestions()
        {
            // Arrange
            var sessionId = 1;
            var positionId = 10;
            var sessionDto = new InterviewSessionDto { Id = sessionId, PositionId = positionId };
            var questionsDto = new List<QuestionDto>
            {
                new QuestionDto { Id = 1, QuestionText = "Q1" },
                new QuestionDto { Id = 2, QuestionText = "Q2" }
            };

            SetupMockResponse(HttpMethod.Get, $"interviewsessions/{sessionId}", HttpStatusCode.OK, sessionDto);
            SetupMockResponse(HttpMethod.Put, $"interviewsessions/{sessionId}", HttpStatusCode.OK);
            SetupMockResponse(HttpMethod.Get, $"questions/byposition/{positionId}", HttpStatusCode.OK, questionsDto);

            // Act
            var (session, questions) = await _service.StartSessionAsync(sessionId);

            // Assert
            Assert.NotNull(session);
            Assert.Equal(sessionId, session.Id);
            Assert.Equal(2, questions.Count);
            Assert.Equal("Q1", questions[0].QuestionText);
        }

        [Fact]
        public async Task SubmitScoreAsync_WhenSessionExists_UpdatesSuccessfully()
        {
            // Arrange
            var sessionId = 1;
            var score = 8.5f;
            var sessionDto = new InterviewSessionDto { Id = sessionId, Status = "Scheduled" };

            SetupMockResponse(HttpMethod.Get, $"interviewsessions/{sessionId}", HttpStatusCode.OK, sessionDto);
            SetupMockResponse(HttpMethod.Put, $"interviewsessions/{sessionId}", HttpStatusCode.OK);

            // Act
            await _service.SubmitScoreAsync(sessionId, score);

            // Assert
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.RequestUri.ToString().EndsWith($"/interviewsessions/{sessionId}")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetScheduledSessionsAsync_WhenSuccessful_ReturnsSessions()
        {
            // Arrange
            var dtos = new List<InterviewSessionDto>
            {
                new InterviewSessionDto { Id = 1 },
                new InterviewSessionDto { Id = 2 }
            };

            SetupMockResponse(HttpMethod.Get, "interviewsessions/scheduled", HttpStatusCode.OK, dtos);

            // Act
            var result = await _service.GetScheduledSessionsAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetScheduledSessionsAsync_WhenNotFound_ReturnsEmptyList()
        {
            // Arrange
            SetupMockResponse(HttpMethod.Get, "interviewsessions/scheduled", HttpStatusCode.NotFound);

            // Act
            var result = await _service.GetScheduledSessionsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteSessionAsync_WhenSuccessful_Completes()
        {
            // Arrange
            var sessionId = 1;
            SetupMockResponse(HttpMethod.Delete, $"interviewsessions/{sessionId}", HttpStatusCode.OK);

            // Act
            await _service.DeleteSessionAsync(sessionId);

            // Assert
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete && req.RequestUri.ToString().EndsWith($"/interviewsessions/{sessionId}")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetSessionsByStatusAsync_WhenSuccessful_ReturnsSessions()
        {
            // Arrange
            var status = "Completed";
            var dtos = new List<InterviewSessionDto> { new InterviewSessionDto { Id = 1, Status = status } };

            SetupMockResponse(HttpMethod.Get, $"interviewsessions/status/{status}", HttpStatusCode.OK, dtos);

            // Act
            var result = await _service.GetSessionsByStatusAsync(status);

            // Assert
            Assert.Single(result);
            Assert.Equal(status, result[0].Status);
        }

        [Fact]
        public async Task GetSessionsByStatusAsync_WhenNotFound_ReturnsEmptyList()
        {
            // Arrange
            var status = "Empty";
            SetupMockResponse(HttpMethod.Get, $"interviewsessions/status/{status}", HttpStatusCode.NotFound);

            // Act
            var result = await _service.GetSessionsByStatusAsync(status);

            // Assert
            Assert.Empty(result);
        }

        private void SetupMockResponse<T>(HttpMethod method, string path, HttpStatusCode statusCode, T content = default)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode
            };

            if (content != null)
            {
                response.Content = JsonContent.Create(content);
            }

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == method && req.RequestUri.ToString().Contains(path)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
        }

        private void SetupMockResponse(HttpMethod method, string path, HttpStatusCode statusCode)
        {
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == method && req.RequestUri.ToString().Contains(path)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode });
        }
    }
}
