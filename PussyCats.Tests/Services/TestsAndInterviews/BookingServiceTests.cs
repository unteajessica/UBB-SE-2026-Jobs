// <copyright file="BookingServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
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

namespace PussyCats.Tests.Services.TestsAndInterviews
{
    public class BookingServiceTests
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

        private Mock<HttpMessageHandler> CreateMockHttpMessageHandlerForMultipleRequests(HttpStatusCode statusCode)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(statusCode))
                .Verifiable();

            return handlerMock;
        }

        [Fact]
        public async Task GetAvailableSlots_ReturnsOnlyFreeSlots()
        {
            var date = DateTime.Today;
            var dtos = new List<SlotDto>
            {
                new SlotDto { StartTime = date, Status = SlotStatus.Free },
                new SlotDto { StartTime = date, Status = SlotStatus.Occupied },
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(dtos)
            };

            var handlerMock = this.CreateMockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
            var bookingService = new BookingService(httpClient);

            var result = await bookingService.GetAvailableSlots(1, date);

            Assert.Single(result);
            Assert.All(result, slot => Assert.Equal(SlotStatus.Free, slot.Status));
        }

        [Fact]
        public async Task GetAvailableSlots_ReturnsSlotsOrderedByStartTime()
        {
            var date = DateTime.Today;
            var dtos = new List<SlotDto>
            {
                new SlotDto { StartTime = date.AddHours(3), Status = SlotStatus.Free },
                new SlotDto { StartTime = date.AddHours(1), Status = SlotStatus.Free },
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(dtos)
            };

            var handlerMock = this.CreateMockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
            var bookingService = new BookingService(httpClient);

            var result = await bookingService.GetAvailableSlots(1, date);

            Assert.Equal(date.AddHours(1), result[0].StartTime);
            Assert.Equal(date.AddHours(3), result[1].StartTime);
        }

        [Fact]
        public async Task GetAvailableSlotsByRecruiterId_ReturnsOnlyFreeSlots()
        {
            var dtos = new List<SlotDto>
            {
                new SlotDto { StartTime = DateTime.Today, Status = SlotStatus.Free },
                new SlotDto { StartTime = DateTime.Today, Status = SlotStatus.Occupied },
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(dtos)
            };

            var handlerMock = this.CreateMockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
            var bookingService = new BookingService(httpClient);

            var result = await bookingService.GetAvailableSlotsByRecruiterId(1);

            Assert.Single(result);
            Assert.All(result, slot => Assert.Equal(SlotStatus.Free, slot.Status));
        }

        [Fact]
        public async Task GetAvailableSlotsByRecruiterId_ReturnsSlotsOrderedByStartTime()
        {
            var dtos = new List<SlotDto>
            {
                new SlotDto { StartTime = DateTime.Today.AddHours(3), Status = SlotStatus.Free },
                new SlotDto { StartTime = DateTime.Today.AddHours(1), Status = SlotStatus.Free },
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(dtos)
            };

            var handlerMock = this.CreateMockHttpMessageHandler(responseMessage);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
            var bookingService = new BookingService(httpClient);

            var result = await bookingService.GetAvailableSlotsByRecruiterId(1);

            Assert.Equal(DateTime.Today.AddHours(1), result[0].StartTime);
            Assert.Equal(DateTime.Today.AddHours(3), result[1].StartTime);
        }

        [Fact]
        public async Task ConfirmBooking_WhenSlotIsNull_ThrowsException()
        {
            // No HTTP calls expected here, so we can use a blank client
            var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object) { BaseAddress = new Uri("http://localhost/api/") };
            var bookingService = new BookingService(httpClient);

            var exception = await Record.ExceptionAsync(() => bookingService.ConfirmBooking(1, null!));

            Assert.NotNull(exception);
            Assert.Equal("Slot not found", exception.Message);
        }

        [Fact]
        public async Task ConfirmBooking_WhenSlotIsNotFree_ThrowsException()
        {
            var slot = new Slot { Status = SlotStatus.Occupied };
            var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object) { BaseAddress = new Uri("http://localhost/api/") };
            var bookingService = new BookingService(httpClient);

            var exception = await Record.ExceptionAsync(() => bookingService.ConfirmBooking(1, slot));

            Assert.NotNull(exception);
            Assert.Equal("This slot is no longer available", exception.Message);
        }

        [Fact]
        public async Task ConfirmBooking_WhenSlotIsFree_UpdatesSlotAndCreatesSession()
        {
            var slot = new Slot
            {
                Id = 5,
                RecruiterId = 2,
                StartTime = DateTime.Today,
                Status = SlotStatus.Free,
            };

            // Setup a handler that returns HTTP 200 OK for any request (PUT and POST)
            var handlerMock = this.CreateMockHttpMessageHandlerForMultipleRequests(HttpStatusCode.OK);
            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
            var bookingService = new BookingService(httpClient);

            await bookingService.ConfirmBooking(1, slot);

            // Assert local slot changes
            Assert.Equal(SlotStatus.Occupied, slot.Status);
            Assert.Equal(1, slot.CandidateId);
            Assert.Equal(string.Empty, slot.InterviewType);

            // Verify PUT request for updating the slot was made
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri!.ToString().EndsWith("slots/5")),
                ItExpr.IsAny<CancellationToken>());

            // Verify POST request for creating the interview session was made
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().EndsWith("interviewsessions")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public void DefaultConstructor_InitializesHttpClient()
        {
            // Act
            var service = new BookingService();

            // Assert
            var httpField = typeof(BookingService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Xunit.Assert.NotNull(httpField);
            Xunit.Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }
    }
}