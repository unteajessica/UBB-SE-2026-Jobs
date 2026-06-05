// <copyright file="SlotServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PussyCats.Tests.Services.TestsAndInterviews
{
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
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services;
    using Xunit;

    /// <summary>
    /// Refactored tests for <see cref="SlotService"/> using HttpClient mocking.
    /// </summary>
    public class SlotServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly SlotService _service;

        // Captured variables for verification
        private string? _lastRequestUri;
        private HttpMethod? _lastMethod;
        private string? _lastPayload;

        public SlotServiceTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            // Default behavior: capture the request and return OK
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    _lastMethod = req.Method;
                    _lastRequestUri = req.RequestUri?.ToString();
                    if (req.Content != null)
                    {
                        _lastPayload = req.Content.ReadAsStringAsync().Result;
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("https://localhost/api/"),
                Timeout = TimeSpan.FromSeconds(2)
            };

            _service = new SlotService(_httpClient);
        }

        // FIX 1: Removed the URL contains check — it conflicted with the default
        // catch-all setup registered first in the constructor, causing the mock to
        // return an empty 200 (no Content) instead of the JSON response.
        // Matching on HttpMethod.Get alone is sufficient and unambiguous.
        private void SetupGetResponse<T>(T responseData)
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(responseData)
                });
        }

        [Fact]
        public async Task LoadRecruiterVisibleSlots_ForExistentRecruiter_ReturnsListWithOccupiedAndFreeSlots()
        {
            // Arrange
            var recruiterId = 1;
            var date = new DateTime(2026, 04, 21);
            var mockSlots = new List<Slot>
            {
                new Slot
                {
                    Id = 1,
                    RecruiterId = recruiterId,
                    StartTime = new DateTime(2026, 04, 21, 8, 0, 0),
                    EndTime = new DateTime(2026, 04, 21, 9, 0, 0),
                    Duration = 60,
                    Status = SlotStatus.Occupied,
                }
            };
            SetupGetResponse(mockSlots);

            // Act
            var recruiterSlots = await _service.LoadRecruiterVisibleSlotsAsync(recruiterId, date);

            // Assert
            var resultOccupiedSlot = recruiterSlots[0];
            var resultFreeSlot = recruiterSlots.Last();

            Assert.Equal(recruiterId, resultOccupiedSlot.RecruiterId);
            Assert.Equal(SlotStatus.Occupied, resultOccupiedSlot.Status);
            Assert.Equal(SlotStatus.Free, resultFreeSlot.Status);
        }

        [Fact]
        public async Task LoadRecruiterVisibleSlots_ForRecruiterWithNoAllocatedSlots_ReturnsListWithFreeSlotsAllDay()
        {
            // Arrange
            var recruiterId = 1;
            var date = new DateTime(2026, 04, 21);
            SetupGetResponse(new List<Slot>());

            // Act
            var recruiterSlots = await _service.LoadRecruiterVisibleSlotsAsync(recruiterId, date);

            // Assert
            Assert.All(recruiterSlots, slot => Assert.Equal(SlotStatus.Free, slot.Status));
            Assert.Equal(20, recruiterSlots.Count); // 8:00 to 18:00 in 30-min increments
        }

        [Fact]
        public async Task CreateNewSlot_FromValidBaseSlot_CallsPostMethodWithCorrectPayload()
        {
            // Arrange
            var mockBaseSlot = new SlotDto
            {
                Id = 0,
                StartTime = new DateTime(2026, 04, 21, 10, 0, 0),
            };
            var duration = 60;

            // Act
            await _service.CreateRecruiterSlotAsync(mockBaseSlot, duration);

            // Assert
            Assert.Equal(HttpMethod.Post, _lastMethod);

            // FIX 2: System.Text.Json serializes with camelCase by default.
            // "Status":0 → "status":0  and  "2026-04-21T10:00:00" stays the same.
            Assert.Contains("\"status\":0", _lastPayload); // SlotStatus.Free
            Assert.Contains("2026-04-21T10:00:00", _lastPayload);
        }

        [Fact]
        public async Task DeleteRecruiterSlot_CallsDeleteMethodWithCorrectIdInUri()
        {
            // Arrange
            var slotToDeleteId = 123;

            // Act
            await _service.DeleteRecruiterSlotAsync(slotToDeleteId);

            // Assert
            Assert.Equal(HttpMethod.Delete, _lastMethod);
            Assert.Contains($"/slots/{slotToDeleteId}", _lastRequestUri);
        }

        [Theory]
        [InlineData(5, 0)]
        [InlineData(21, 0)]
        public async Task UpdateRecruiterSlot_InvalidNewStartTime_ThrowsExceptionBeforeNetworkCall(int newStartTimeHours, int newStartTimeMinutes)
        {
            // Arrange
            var initialSlot = new SlotDto
            {
                Id = 1,
                RecruiterId = 1,
                StartTime = new DateTime(2026, 04, 21, 10, 30, 0),
            };
            var newStartTime = new DateTime(2026, 04, 21, newStartTimeHours, newStartTimeMinutes, 0);
            var duration = 30;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _service.UpdateRecruiterSlotAsync(initialSlot, newStartTime, duration));

            Assert.Null(_lastMethod); // Ensure no HTTP call was actually made
        }

        [Fact]
        public async Task UpdateRecruiterSlot_ValidNewStartTimeAndDuration_CallsPutMethodWithCorrectPayload()
        {
            // Arrange
            var initialSlot = new SlotDto
            {
                Id = 55,
                RecruiterId = 1,
                StartTime = new DateTime(2026, 04, 21, 10, 30, 0),
            };
            var newStartTime = new DateTime(2026, 04, 21, 12, 0, 0);
            var duration = 30;

            // Act
            await _service.UpdateRecruiterSlotAsync(initialSlot, newStartTime, duration);

            // Assert
            Assert.Equal(HttpMethod.Put, _lastMethod);

            // FIX 3: System.Text.Json serializes with camelCase by default.
            // "Id":55 → "id":55  and  the StartTime value stays the same.
            Assert.Contains("\"id\":55", _lastPayload);
            Assert.Contains("2026-04-21T12:00:00", _lastPayload);
        }

        [Fact]
        public void DefaultConstructor_InitializesHttpClient()
        {
            // Act
            var service = new SlotService();

            // Assert
            var httpField = typeof(SlotService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }

        [Fact]
        public void ParametrizedConstructor_WithNull_InitializesWithDefault()
        {
            // Act
            var service = new SlotService(null!);

            // Assert
            var httpField = typeof(SlotService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }
    }
}