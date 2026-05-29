// <copyright file="DataProcessingServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews.Tests.Services
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

    public class DataProcessingServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly DataProcessingService sut;

        private TestAttemptDto? _lastPutDto;
        private int _putCallCount;

        public DataProcessingServiceTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            _putCallCount = 0;
            _lastPutDto = null;

            // Intercept PUT requests to capture the updated TestAttemptDto
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    _putCallCount++;
                    if (req.Content != null)
                    {
                        var json = req.Content.ReadAsStringAsync().Result;
                        _lastPutDto = System.Text.Json.JsonSerializer.Deserialize<TestAttemptDto>(
                            json,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Default GET to 404
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("https://localhost/api/")
            };

            sut = new DataProcessingService(_httpClient);
        }

        #region helpers

        private static TestAttemptDto MakeValidAttemptDto() =>
            new TestAttemptDto
            {
                Id = 1,
                TestId = 10,
                ExternalUserId = 5,
                Score = 80m,
                Status = "COMPLETED",
                CompletedAt = DateTime.UtcNow,
            };

        private static UserDto MakeUserDto() =>
            new UserDto { Id = 5 };

        private static TestDto MakeRecentTestDto() =>
            new TestDto { Id = 10, CreatedAt = DateTime.UtcNow };

        private static TestDto MakeExpiredTestDto() =>
            new TestDto { Id = 10, CreatedAt = DateTime.UtcNow.AddMonths(-4) };

        private void SetupGet<T>(string uriFragment, HttpStatusCode statusCode, T? content)
        {
            var response = new HttpResponseMessage(statusCode);
            if (content != null)
            {
                response.Content = JsonContent.Create(content);
            }

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().EndsWith(uriFragment)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
        }

        /// <summary>
        /// Sets up all mock HTTP GET responses needed to reach the happy path.
        /// Individual tests break exactly one thing on top of this.
        /// </summary>
        private void SetupValidFlow(TestAttemptDto attempt, UserDto? user, TestDto? test)
        {
            SetupGet($"testattempts/{attempt.Id}", HttpStatusCode.OK, attempt);

            if (attempt.ExternalUserId.HasValue)
            {
                if (user != null)
                {
                    SetupGet($"users/{attempt.ExternalUserId.Value}", HttpStatusCode.OK, user);
                }
                else
                {
                    SetupGet($"users/{attempt.ExternalUserId.Value}", HttpStatusCode.NotFound, (UserDto?)null);
                }
            }

            if (test != null)
            {
                SetupGet($"tests/{attempt.TestId}", HttpStatusCode.OK, test);
            }
            else
            {
                SetupGet($"tests/{attempt.TestId}", HttpStatusCode.NotFound, (TestDto?)null);
            }
        }

        #endregion


        [Fact]
        public async Task ProcessFinalizedAttempt_WhenExternalUserIdIsNull_ReturnsFalse()
        {
            var attempt = MakeValidAttemptDto();
            attempt.ExternalUserId = null;
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenUserNotFound_ReturnsFalse()
        {
            var attempt = MakeValidAttemptDto();
            SetupValidFlow(attempt, null, MakeRecentTestDto());

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenTestNotFound_ReturnsFalse()
        {
            var attempt = MakeValidAttemptDto();
            SetupValidFlow(attempt, MakeUserDto(), null);

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenCompletedAtIsNull_ReturnsFalse()
        {
            var attempt = MakeValidAttemptDto();
            attempt.CompletedAt = null;
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenStatusIsNull_ReturnsFalse()
        {
            var attempt = MakeValidAttemptDto();
            attempt.Status = null;
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenStatusIsWhitespace_ReturnsFalse()
        {
            var attempt = MakeValidAttemptDto();
            attempt.Status = "   ";
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenStatusIsNotCompleted_ReturnsFalse()
        {
            var attempt = MakeValidAttemptDto();
            attempt.Status = "IN_PROGRESS";
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenScoreIsNegative_ReturnsFalse()
        {
            var attempt = MakeValidAttemptDto();
            attempt.Score = -1m;
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenScoreExceedsMaximum_ReturnsFalse()
        {
            var attempt = MakeValidAttemptDto();
            attempt.Score = 101m;
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenTestIsExpired_ReturnsFalse()
        {
            var attempt = MakeValidAttemptDto();
            SetupValidFlow(attempt, MakeUserDto(), MakeExpiredTestDto());

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenAttemptIsValid_ReturnsTrue()
        {
            var attempt = MakeValidAttemptDto();
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenAttemptIsValid_SetsIsValidatedTrue()
        {
            var attempt = MakeValidAttemptDto();
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.NotNull(_lastPutDto);
            Assert.True(_lastPutDto!.IsValidated);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenAttemptIsValid_SetsPercentageScore()
        {
            var attempt = MakeValidAttemptDto();
            attempt.Score = 80m;
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.NotNull(_lastPutDto);
            Assert.Equal(80m, _lastPutDto!.PercentageScore);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenAttemptIsValid_ClearsRejectionReason()
        {
            var attempt = MakeValidAttemptDto();
            attempt.RejectionReason = "previously rejected";
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.NotNull(_lastPutDto);
            Assert.Null(_lastPutDto!.RejectionReason);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenAttemptIsValid_ClearsRejectedAt()
        {
            var attempt = MakeValidAttemptDto();
            attempt.RejectedAt = DateTime.UtcNow.AddDays(-1);
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.NotNull(_lastPutDto);
            Assert.Null(_lastPutDto!.RejectedAt);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenValidationFails_SetsIsValidatedFalse()
        {
            var attempt = MakeValidAttemptDto();
            attempt.ExternalUserId = null;
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.NotNull(_lastPutDto);
            Assert.False(_lastPutDto!.IsValidated);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenValidationFails_ClearsPercentageScore()
        {
            var attempt = MakeValidAttemptDto();
            attempt.ExternalUserId = null;
            attempt.PercentageScore = 80m;
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.NotNull(_lastPutDto);
            Assert.Null(_lastPutDto!.PercentageScore);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenValidationFails_SetsRejectedAt()
        {
            var attempt = MakeValidAttemptDto();
            attempt.ExternalUserId = null;
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            var before = DateTime.UtcNow;
            await sut.ProcessFinalizedAttemptAsync(attempt.Id);
            var after = DateTime.UtcNow;

            Assert.NotNull(_lastPutDto);
            Assert.NotNull(_lastPutDto!.RejectedAt);
            Assert.InRange(_lastPutDto.RejectedAt!.Value, before, after);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenValidationFails_CallsUpdate()
        {
            var attempt = MakeValidAttemptDto();
            attempt.ExternalUserId = null;
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.Equal(1, _putCallCount);
        }

        [Fact]
        public async Task ProcessFinalizedAttempt_WhenStatusIsCompletedLowercase_ReturnsTrue()
        {
            var attempt = MakeValidAttemptDto();
            attempt.Status = "completed";
            SetupValidFlow(attempt, MakeUserDto(), MakeRecentTestDto());

            var result = await sut.ProcessFinalizedAttemptAsync(attempt.Id);

            Assert.True(result);
        }

        [Fact]
        public void DefaultConstructor_InitializesHttpClient()
        {
            // Act
            var service = new DataProcessingService();

            // Assert
            var httpField = typeof(DataProcessingService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }

        [Fact]
        public void ParametrizedConstructor_WithNull_InitializesWithDefault()
        {
            // Act
            var service = new DataProcessingService(null!);

            // Assert
            var httpField = typeof(DataProcessingService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }
    }
}