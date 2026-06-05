// <copyright file="TimerServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PussyCats.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services;
    using Xunit;

    /// <summary>
    /// Contains unit tests for the <see cref="TimerService"/> class using HttpClient mocks.
    /// </summary>
    public class TimerServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;

        // Captured variables for API verification
        private TestAttempt? _lastPutAttempt;
        private string? _lastPutUri;
        private int _putCallCount;

        public TimerServiceTests()
        {
            this._mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            this._putCallCount = 0;
            this._lastPutAttempt = null;

            // Intercept PUT requests (simulating the Update call)
            this._mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    this._putCallCount++;
                    this._lastPutUri = req.RequestUri?.ToString();
                    if (req.Content != null)
                    {
                        var json = req.Content.ReadAsStringAsync().Result;
                        this._lastPutAttempt = System.Text.Json.JsonSerializer.Deserialize<TestAttempt>(
                            json,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            this._httpClient = new HttpClient(this._mockHandler.Object)
            {
                BaseAddress = new Uri("https://localhost/api/")
            };
        }

        private TimerService MakeTimerService()
        {
            // Now passing HttpClient instead of ITestAttemptRepository
            return new TimerService(this._httpClient);
        }

        [Fact]
        public void CheckExpiration_WhenTimerNotStarted_ReturnsFalse()
        {
            // Arrange
            var timerService = this.MakeTimerService();

            // Act
            bool result = timerService.CheckExpiration(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CheckExpiration_WhenTimerJustStarted_ReturnsFalse()
        {
            // Arrange
            var timerService = this.MakeTimerService();
            timerService.StartTimer(1);

            // Act
            bool result = timerService.CheckExpiration(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetExpiredAttemptIds_WhenNoTimersStarted_ReturnsEmptyList()
        {
            // Arrange
            var timerService = this.MakeTimerService();

            // Act
            List<int> result = timerService.GetExpiredAttemptIds();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetExpiredAttemptIds_WhenTimerJustStarted_DoesNotIncludeAttempt()
        {
            // Arrange
            var timerService = this.MakeTimerService();
            timerService.StartTimer(2);

            // Act
            List<int> result = timerService.GetExpiredAttemptIds();

            // Assert
            Assert.DoesNotContain(2, result);
        }

        [Fact]
        public async Task ExpireTestAsync_WhenCalled_SendsPutRequestToApi()
        {
            // Arrange
            var timerService = this.MakeTimerService();
            timerService.StartTimer(1);

            // Act
            await timerService.ExpireTestAsync(1);

            // Assert
            Assert.Equal(1, this._putCallCount);
            Assert.NotNull(this._lastPutUri);
            Assert.Contains("testattempts/1", this._lastPutUri);
        }

        [Fact]
        public async Task ExpireTestAsync_WhenCalled_SetsStatusToCompletedInPayload()
        {
            // Arrange
            var timerService = this.MakeTimerService();
            timerService.StartTimer(1);

            // Act
            await timerService.ExpireTestAsync(1);

            // Assert
            Assert.NotNull(this._lastPutAttempt);
            Assert.Equal(TestStatus.COMPLETED.ToString(), this._lastPutAttempt!.Status);
        }

        [Fact]
        public async Task ExpireTestAsync_WhenCalled_SetsCompletedAtInPayload()
        {
            // Arrange
            var timerService = this.MakeTimerService();
            timerService.StartTimer(1);

            // Act
            await timerService.ExpireTestAsync(1);

            // Assert
            Assert.NotNull(this._lastPutAttempt!.CompletedAt);
            Assert.True(this._lastPutAttempt.CompletedAt <= DateTime.UtcNow);
        }

        [Fact]
        public async Task ExpireTestAsync_WhenCalled_RemovesTimerFromActiveTimers()
        {
            // Arrange
            var timerService = this.MakeTimerService();
            timerService.StartTimer(1);

            // Act
            await timerService.ExpireTestAsync(1);

            // Assert
            // CheckExpiration should be false because the internal state was cleared
            Assert.False(timerService.CheckExpiration(1));
        }

        [Fact]
        public void StartTimer_WhenCalledTwiceForSameAttempt_OverwritesPreviousTimer()
        {
            // Arrange
            var timerService = this.MakeTimerService();

            // Act
            timerService.StartTimer(1);
            timerService.StartTimer(1);

            // Assert
            Assert.False(timerService.CheckExpiration(1));
        }

        [Fact]
        public void DefaultConstructor_InitializesHttpClient()
        {
            // Act
            var service = new TimerService();

            // Assert
            var httpField = typeof(TimerService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }

        [Fact]
        public void ParametrizedConstructor_WithNull_InitializesWithDefault()
        {
            // Act
            var service = new TimerService(null!);

            // Assert
            var httpField = typeof(TimerService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }
    }
}