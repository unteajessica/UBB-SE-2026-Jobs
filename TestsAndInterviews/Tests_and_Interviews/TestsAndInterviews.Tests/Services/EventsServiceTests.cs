// <copyright file="EventsServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TestsAndInterviews.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services;
    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class EventsServiceTests
    {
        private const string DefaultPhoto = "photo.jpg";
        private const string UpdatedPhoto = "new_photo.jpg";

        private const string DefaultTitle = "Test Event";
        private const string TitleHackathon = "Hackathon";
        private const string TitleGeneric = "Event";
        private const string TitleSpecific = "Event Title";
        private const string TitleUpdated = "Updated Title";
        private const string TitleShort = "Title";

        private const string DefaultDesc = "Test Description";
        private const string DescLower = "description";
        private const string DescSpecific = "Description for event";
        private const string DescShort = "desc";
        private const string DescEvent = "Event Description";
        private const string DescUpdated = "Updated description";

        private const string LocClujNapoca = "Cluj-Napoca";
        private const string LocCluj = "Cluj";
        private const string LocTimisoara = "Timisoara";

        private const string CollabName = "Collaborator";

        private const int Year = 2026;
        private const int MonthJune = 6;
        private const int MonthJuly = 7;

        private const int Day1 = 1;
        private const int Day2 = 2;
        private const int Day3 = 3;
        private const int Day10 = 10;
        private const int Day15 = 15;

        private const int DefaultId = 1;
        private const int ExpectedHostId = 42;
        private const int AltEventId = 5;
        private const int CollabId = 10;

        private const int CountZero = 0;
        private const int CountOne = 1;
        private const int CountTwo = 2;

        private Mock<HttpMessageHandler> _mockHandler = null!;
        private HttpClient _httpClient = null!;
        private EventsService eventsService = null!;

        // Captured variables for verifying HTTP requests
        private EventDto? _lastPostedDto;
        private string? _lastPostUri;
        private EventDto? _lastPutDto;
        private string? _lastPutUri;
        private string? _lastDeleteUri;

        private static Event MakeEvent()
        {
            return new Event(
                DefaultPhoto,
                DefaultTitle,
                DefaultDesc,
                new DateTime(Year, MonthJune, Day1),
                new DateTime(Year, MonthJune, Day2),
                LocClujNapoca,
                DefaultId);
        }

        [TestInitialize]
        public void Setup()
        {
            _lastPostedDto = null;
            _lastPostUri = null;
            _lastPutDto = null;
            _lastPutUri = null;
            _lastDeleteUri = null;

            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            // Intercept POST
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    _lastPostUri = req.RequestUri?.ToString();
                    if (req.Content != null)
                    {
                        var json = req.Content.ReadAsStringAsync().Result;
                        _lastPostedDto = System.Text.Json.JsonSerializer.Deserialize<EventDto>(
                            json,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                })
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    // Return the payload back to simulate successful API creation mapping
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(_lastPostedDto))
                });

            // Intercept PUT
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    _lastPutUri = req.RequestUri?.ToString();
                    if (req.Content != null)
                    {
                        var json = req.Content.ReadAsStringAsync().Result;
                        _lastPutDto = System.Text.Json.JsonSerializer.Deserialize<EventDto>(
                            json,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Intercept DELETE
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    _lastDeleteUri = req.RequestUri?.ToString();
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Default GET fallback
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

            eventsService = new EventsService(_httpClient);
        }

        [TestMethod]
        public void Constructor_WithoutArgs_DoesNotThrow()
        {
            var service = new EventsService();
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Constructor_WithNullHttpClient_DoesNotThrow()
        {
            var service = new EventsService(null!);
            Assert.IsNotNull(service);
        }

        private void SetupGetEventsResponse(string uriFragment, List<EventDto> dtos)
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains(uriFragment)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(dtos)
                });
        }

        // Note: Changed void tests to async Task to await HttpClient operations

        [TestMethod]
        public async Task AddEvent_ValidData_SendsPostRequest()
        {
            await eventsService.AddEvent(DefaultPhoto, TitleShort, DescLower,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());

            Assert.IsNotNull(_lastPostedDto);
            Assert.IsTrue(_lastPostUri!.Contains("events"));
        }

        [TestMethod]
        public async Task AddEvent_ValidData_EventPassedToApiHasCorrectTitle()
        {
            await eventsService.AddEvent(DefaultPhoto, TitleHackathon, DescLower,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());

            Assert.AreEqual(TitleHackathon, _lastPostedDto?.Title);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_EventPassedToApiHasCorrectDescription()
        {
            await eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());

            Assert.AreEqual(DescSpecific, _lastPostedDto?.Description);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_EventPassedToApiHasCorrectStartDate()
        {
            DateTime expectedStartDate = new DateTime(Year, MonthJune, Day1);
            await eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                expectedStartDate, new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());

            Assert.AreEqual(expectedStartDate, _lastPostedDto?.StartDate);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_EventPassedToApiHasCorrectEndDate()
        {
            DateTime expectedEndDate = new DateTime(Year, MonthJune, Day3);
            await eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), expectedEndDate, LocCluj, DefaultId, new List<Company>());

            Assert.AreEqual(expectedEndDate, _lastPostedDto?.EndDate);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_EventPassedToApiHasCorrectLocation()
        {
            await eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());

            Assert.AreEqual(LocCluj, _lastPostedDto?.Location);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_EventPassedToApiHasCorrectHostId()
        {
            await eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, ExpectedHostId, new List<Company>());

            Assert.AreEqual(ExpectedHostId, _lastPostedDto?.HostCompanyId);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_ReturnsTheCreatedEvent()
        {
            Event result = await eventsService.AddEvent(DefaultPhoto, TitleShort, DescLower,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_ReturnedEventHasCorrectTitle()
        {
            Event result = await eventsService.AddEvent(DefaultPhoto, TitleSpecific, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());

            Assert.AreEqual(TitleSpecific, result.Title);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_ReturnedEventHasCorrectDescription()
        {
            Event result = await eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescEvent,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());

            Assert.AreEqual(DescEvent, result.Description);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_ReturnedEventHasCorrectStartDate()
        {
            DateTime expectedStartDate = new DateTime(Year, MonthJune, Day1);
            Event result = await eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                expectedStartDate, new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());

            Assert.AreEqual(expectedStartDate, result.StartDate);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_ReturnedEventHasCorrectEndDate()
        {
            DateTime expectedEndDate = new DateTime(Year, MonthJune, Day3);
            Event result = await eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), expectedEndDate, LocCluj, DefaultId, new List<Company>());

            Assert.AreEqual(expectedEndDate, result.EndDate);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_ReturnedEventHasCorrectLocation()
        {
            Event result = await eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, DefaultId, new List<Company>());

            Assert.AreEqual(LocCluj, result.Location);
        }

        [TestMethod]
        public async Task AddEvent_ValidData_ReturnedEventHasCorrectHostId()
        {
            Event result = await eventsService.AddEvent(DefaultPhoto, TitleGeneric, DescSpecific,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj, ExpectedHostId, new List<Company>());

            Assert.AreEqual(ExpectedHostId, result.HostCompanyId);
        }

        

        [TestMethod]
        public async Task DeleteEvent_ValidEvent_CorrectEventIdPassedToApiForDeletion()
        {
            Event eventToDelete = MakeEvent();

            await eventsService.DeleteEvent(eventToDelete);

            Assert.IsNotNull(_lastDeleteUri);
            Assert.IsTrue(_lastDeleteUri!.EndsWith($"events/{eventToDelete.Id}"));
        }

        [TestMethod]
        public async Task UpdateEvent_ValidData_ApiReceivesCorrectEventIdInUri()
        {
            await eventsService.UpdateEvent(AltEventId, DefaultPhoto, TitleShort, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj);

            Assert.IsNotNull(_lastPutUri);
            Assert.IsTrue(_lastPutUri!.EndsWith($"events/{AltEventId}"));
        }

        [TestMethod]
        public async Task UpdateEvent_ValidData_ApiReceivesCorrectPhoto()
        {
            await eventsService.UpdateEvent(DefaultId, UpdatedPhoto, TitleShort, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj);

            // Assuming the DTO maps PhotoPath or EventPhotoPath
            var json = System.Text.Json.JsonSerializer.Serialize(_lastPutDto);
            Assert.IsTrue(json.Contains(UpdatedPhoto));
        }

        [TestMethod]
        public async Task UpdateEvent_ValidData_ApiReceivesCorrectTitle()
        {
            await eventsService.UpdateEvent(DefaultId, DefaultPhoto, TitleUpdated, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj);

            Assert.AreEqual(TitleUpdated, _lastPutDto?.Title);
        }

        [TestMethod]
        public async Task UpdateEvent_ValidData_ApiReceivesCorrectDescription()
        {
            await eventsService.UpdateEvent(DefaultId, DefaultPhoto, TitleShort, DescUpdated,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocCluj);

            Assert.AreEqual(DescUpdated, _lastPutDto?.Description);
        }

        [TestMethod]
        public async Task UpdateEvent_ValidData_ApiReceivesCorrectStartDate()
        {
            DateTime expectedStartDate = new DateTime(Year, MonthJuly, Day10);
            await eventsService.UpdateEvent(DefaultId, DefaultPhoto, TitleShort, DescShort,
                expectedStartDate, new DateTime(Year, MonthJuly, Day15), LocCluj);

            Assert.AreEqual(expectedStartDate, _lastPutDto?.StartDate);
        }

        [TestMethod]
        public async Task UpdateEvent_ValidData_ApiReceivesCorrectEndDate()
        {
            DateTime expectedEndDate = new DateTime(Year, MonthJuly, Day15);
            await eventsService.UpdateEvent(DefaultId, DefaultPhoto, TitleShort, DescShort,
                new DateTime(Year, MonthJuly, Day10), expectedEndDate, LocCluj);

            Assert.AreEqual(expectedEndDate, _lastPutDto?.EndDate);
        }

        [TestMethod]
        public async Task UpdateEvent_ValidData_ApiReceivesCorrectLocation()
        {
            await eventsService.UpdateEvent(DefaultId, DefaultPhoto, TitleShort, DescShort,
                new DateTime(Year, MonthJune, Day1), new DateTime(Year, MonthJune, Day2), LocTimisoara);

            Assert.AreEqual(LocTimisoara, _lastPutDto?.Location);
        }

        [TestMethod]
        public async Task GetCurrentEvents_ApiReturnsTwoEvents_ServiceReturnsTwoEvents()
        {
            var dtos = new List<EventDto>
            {
                new EventDto { Id = 1, Title = DefaultTitle },
                new EventDto { Id = 2, Title = TitleHackathon }
            };
            SetupGetEventsResponse($"events/current/{DefaultId}", dtos);

            var result = await eventsService.GetCurrentEvents(DefaultId);

            Assert.AreEqual(CountTwo, result.Count);
        }

        [TestMethod]
        public async Task GetCurrentEvents_ApiReturnsEmptyCollection_ServiceReturnsEmptyCollection()
        {
            SetupGetEventsResponse($"events/current/{DefaultId}", new List<EventDto>());

            var result = await eventsService.GetCurrentEvents(DefaultId);

            Assert.AreEqual(CountZero, result.Count);
        }

        [TestMethod]
        public async Task GetPastEvents_ApiReturnsTwoEvents_ServiceReturnsTwoEvents()
        {
            var dtos = new List<EventDto>
            {
                new EventDto { Id = 1, Title = DefaultTitle },
                new EventDto { Id = 2, Title = TitleHackathon }
            };
            SetupGetEventsResponse($"events/past/{DefaultId}", dtos);

            var result = await eventsService.GetPastEvents(DefaultId);

            Assert.AreEqual(CountTwo, result.Count);
        }

        [TestMethod]
        public async Task GetPastEvents_ApiReturnsEmptyCollection_ServiceReturnsEmptyCollection()
        {
            SetupGetEventsResponse($"events/past/{DefaultId}", new List<EventDto>());

            var result = await eventsService.GetPastEvents(DefaultId);

            Assert.AreEqual(CountZero, result.Count);
        }
    }
}