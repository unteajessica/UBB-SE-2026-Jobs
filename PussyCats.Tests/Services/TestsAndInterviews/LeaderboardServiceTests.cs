// <copyright file="LeaderboardServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;
using Tests_and_Interviews.Dtos;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;

namespace PussyCats.Tests.Services.TestsAndInterviews
{
    public class LeaderboardServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly LeaderboardService _leaderboardService;

        private List<LeaderboardEntry>? _lastPostedEntries;
        private List<string> _callOrder;
        private int _deleteCallCount;
        private int _postCallCount;

        public LeaderboardServiceTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            _callOrder = new List<string>();
            _lastPostedEntries = null;
            _deleteCallCount = 0;
            _postCallCount = 0;

            // Intercept DELETE
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    _callOrder.Add("delete");
                    _deleteCallCount++;
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Intercept POST — service POSTs to "leaderboard" (no 's', no "/range")
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    _callOrder.Add("save");
                    _postCallCount++;
                    if (req.Content != null)
                    {
                        var json = req.Content.ReadAsStringAsync().Result;
                        _lastPostedEntries = System.Text.Json.JsonSerializer.Deserialize<List<LeaderboardEntry>>(
                            json,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("https://localhost/api/")
            };

            _leaderboardService = new LeaderboardService(_httpClient);
        }

        #region helpers

        // The service deserializes GET attempts as List<TestAttemptDto>, not List<TestAttempt>,
        // so all mock setup must return DTOs.
        private static TestAttemptDto MakeAttemptDto(int testId, int userId, decimal pct, DateTime? completedAt = null) =>
            new TestAttemptDto
            {
                TestId = testId,
                ExternalUserId = userId,
                PercentageScore = pct,
                Score = pct,
                StartedAt = DateTime.UtcNow.AddMinutes(-30),
                CompletedAt = completedAt ?? DateTime.UtcNow,
            };

        // The service deserializes GET leaderboard responses as List<LeaderboardEntryDto>
        // and maps them via .ToEntity(), so mocks must return DTOs here too.
        private static LeaderboardEntryDto MakeEntryDto(int testId, int userId, decimal score, int rank) =>
            new LeaderboardEntryDto
            {
                TestId = testId,
                UserId = userId,
                NormalizedScore = score,
                RankPosition = rank,
            };

        // Exact full-URI equality so multiple GET setups never shadow each other.
        // .Contains() and .EndsWith() both break when one URL is a prefix of another
        // (e.g. "leaderboard/bytest/30" vs "leaderboard/bytest/30/top/3").
        private void SetupGetResponse<T>(string uriPath, T? content)
        {
            var fullUri = $"https://localhost/api/{uriPath}";

            var response = new HttpResponseMessage(
                content == null ? HttpStatusCode.NotFound : HttpStatusCode.OK);

            if (content != null)
            {
                response.Content = JsonContent.Create(content);
            }

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == fullUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }

        private void SetupRecalculate(int testId, List<TestAttemptDto> attemptDtos)
        {
            SetupGetResponse($"testattempts/valid/bytest/{testId}", attemptDtos);
        }

        #endregion

        #region RecalculateLeaderboard

        [Fact]
        public async Task RecalculateLeaderboard_WithAttempts_DeletesThenSavesEntries()
        {
            const int testId = 1;

            var attempts = new List<TestAttemptDto>
            {
                MakeAttemptDto(testId, 10, 90m),
                MakeAttemptDto(testId, 20, 75m),
            };

            SetupRecalculate(testId, attempts);

            await _leaderboardService.RecalculateLeaderboardAsync(testId);

            Assert.Equal(1, _deleteCallCount);
            Assert.Equal(1, _postCallCount);
            Assert.NotNull(_lastPostedEntries);
            Assert.Equal(2, _lastPostedEntries!.Count);
        }

        [Fact]
        public async Task RecalculateLeaderboard_AssignsRanksInOrder()
        {
            const int testId = 2;

            var attempts = new List<TestAttemptDto>
            {
                MakeAttemptDto(testId, 1, 95m),
                MakeAttemptDto(testId, 2, 80m),
                MakeAttemptDto(testId, 3, 60m),
            };

            SetupRecalculate(testId, attempts);

            await _leaderboardService.RecalculateLeaderboardAsync(testId);

            Assert.NotNull(_lastPostedEntries);
            Assert.Equal(1, _lastPostedEntries![0].RankPosition);
            Assert.Equal(2, _lastPostedEntries[1].RankPosition);
            Assert.Equal(3, _lastPostedEntries[2].RankPosition);
        }

        [Fact]
        public async Task RecalculateLeaderboard_MapsAttemptFieldsToEntryCorrectly()
        {
            const int testId = 3;
            const int userId = 10;
            const decimal pct = 88.5m;

            var attempts = new List<TestAttemptDto> { MakeAttemptDto(testId, userId, pct) };

            SetupRecalculate(testId, attempts);

            var before = DateTime.UtcNow;
            await _leaderboardService.RecalculateLeaderboardAsync(testId);
            var after = DateTime.UtcNow;

            Assert.NotNull(_lastPostedEntries);
            var entry = _lastPostedEntries![0];
            Assert.Equal(testId, entry.TestId);
            Assert.Equal(userId, entry.UserId);
            Assert.Equal(pct, entry.NormalizedScore);
            Assert.Equal(1, entry.RankPosition);
            Assert.Equal(1, entry.TieBreakPriority);
            Assert.InRange(entry.LastRecalculationAt, before, after);
        }

        [Fact]
        public async Task RecalculateLeaderboard_WithNoAttempts_DeletesButDoesNotSave()
        {
            const int testId = 4;

            SetupRecalculate(testId, new List<TestAttemptDto>());

            await _leaderboardService.RecalculateLeaderboardAsync(testId);

            Assert.Equal(1, _deleteCallCount);
            Assert.Equal(0, _postCallCount);
            Assert.Null(_lastPostedEntries);
        }

        [Fact]
        public async Task RecalculateLeaderboard_DeleteAlwaysCalledBeforeSave()
        {
            const int testId = 5;

            SetupRecalculate(testId, new List<TestAttemptDto> { MakeAttemptDto(testId, 99, 50m) });

            await _leaderboardService.RecalculateLeaderboardAsync(testId);

            Assert.Equal(new[] { "delete", "save" }, _callOrder);
        }

        #endregion

        #region GetTopThree

        [Fact]
        public async Task GetTopThree_RecalculatesThenReturnsTopThree()
        {
            const int testId = 10;

            var attempts = new List<TestAttemptDto>
            {
                MakeAttemptDto(testId, 1, 95m),
                MakeAttemptDto(testId, 2, 85m),
                MakeAttemptDto(testId, 3, 60m),
                MakeAttemptDto(testId, 4, 75m),
            };

            // Service GETs "leaderboard/bytest/{id}/top/3" (no 's')
            var topThree = new List<LeaderboardEntryDto>
            {
                MakeEntryDto(testId, 1, 95m, 1),
                MakeEntryDto(testId, 2, 85m, 2),
                MakeEntryDto(testId, 4, 75m, 3),
            };

            SetupRecalculate(testId, attempts);
            SetupGetResponse($"leaderboard/bytest/{testId}/top/3", topThree);

            var result = await _leaderboardService.GetTopThreeAsync(testId);

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].RankPosition);
        }

        [Fact]
        public async Task GetTopThree_WithFewerThanThreeAttempts_ReturnsWhatExists()
        {
            const int testId = 11;

            SetupRecalculate(testId, new List<TestAttemptDto> { MakeAttemptDto(testId, 1, 70m) });

            var singleEntry = new List<LeaderboardEntryDto> { MakeEntryDto(testId, 1, 70m, 1) };
            SetupGetResponse($"leaderboard/bytest/{testId}/top/3", singleEntry);

            var result = await _leaderboardService.GetTopThreeAsync(testId);

            Assert.Single(result);
        }

        #endregion

        #region GetUserRanking

        [Fact]
        public async Task GetUserRanking_RecalculatesThenReturnsEntry()
        {
            const int testId = 20;
            const int userId = 5;

            SetupRecalculate(testId, new List<TestAttemptDto> { MakeAttemptDto(testId, userId, 82m) });

            // Service GETs "leaderboard/bytest/{testId}/byuser/{userId}"
            // (bytest first, byuser second — opposite of the original test)
            var expectedEntry = MakeEntryDto(testId, userId, 82m, 1);
            SetupGetResponse($"leaderboard/bytest/{testId}/byuser/{userId}", expectedEntry);

            var result = await _leaderboardService.GetUserRankingAsync(userId, testId);

            Assert.NotNull(result);
            Assert.Equal(userId, result!.UserId);
        }

        [Fact]
        public async Task GetUserRanking_WhenUserHasNoEntry_ReturnsNull()
        {
            const int testId = 21;
            const int userId = 99;

            SetupRecalculate(testId, new List<TestAttemptDto>());

            SetupGetResponse($"leaderboard/bytest/{testId}/byuser/{userId}", (LeaderboardEntryDto?)null);

            var result = await _leaderboardService.GetUserRankingAsync(userId, testId);

            Assert.Null(result);
        }

        #endregion

        #region GetFullLeaderboard

        [Fact]
        public async Task GetFullLeaderboard_RecalculatesThenReturnsAll()
        {
            const int testId = 30;

            var attempts = new List<TestAttemptDto>
            {
                MakeAttemptDto(testId, 1, 91m),
                MakeAttemptDto(testId, 2, 78m),
                MakeAttemptDto(testId, 3, 55m),
                MakeAttemptDto(testId, 4, 30m),
            };

            SetupRecalculate(testId, attempts);

            // Service GETs "leaderboard/bytest/{id}" (no 's')
            var fullBoard = new List<LeaderboardEntryDto>
            {
                MakeEntryDto(testId, 1, 91m, 1),
                MakeEntryDto(testId, 2, 78m, 2),
                MakeEntryDto(testId, 3, 55m, 3),
                MakeEntryDto(testId, 4, 30m, 4),
            };

            SetupGetResponse($"leaderboard/bytest/{testId}", fullBoard);

            var result = await _leaderboardService.GetFullLeaderboardAsync(testId);

            Assert.Equal(4, result.Count);
        }

        [Fact]
        public async Task GetFullLeaderboard_WhenNoAttempts_ReturnsEmptyList()
        {
            const int testId = 31;

            SetupRecalculate(testId, new List<TestAttemptDto>());

            SetupGetResponse($"leaderboard/bytest/{testId}", new List<LeaderboardEntryDto>());

            var result = await _leaderboardService.GetFullLeaderboardAsync(testId);

            Assert.Empty(result);
        }

        #endregion

        #region ConstructorTests

        [Fact]
        public void DefaultConstructor_InitializesHttpClient()
        {
            // Act
            var service = new LeaderboardService();

            // Assert
            var httpField = typeof(LeaderboardService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }

        [Fact]
        public void ParametrizedConstructor_WithNull_InitializesWithDefault()
        {
            // Act
            var service = new LeaderboardService(null!);

            // Assert
            var httpField = typeof(LeaderboardService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }

        #endregion
    }
}