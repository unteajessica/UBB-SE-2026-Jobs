// <copyright file="CollaboratorsServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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
using Tests_and_Interviews.Services;
using Xunit;
namespace PussyCats.Tests.Services.TestsAndInterviews
{
    public class CollaboratorsServiceTests
    {
        private const string EventPhotoPath = "photo.jpg";
        private const string EventTitle = "Test Event";
        private const string EventDescription = "Test Description";
        private const string EventLocation = "Cluj-Napoca";

        private const string DefaultCompanyName = "Company";
        private const string AltCompanyName = "Corporation";
        private const string Company1Name = "Company1";
        private const string Company2Name = "Company2";
        private const string SingleCompanyName = "Corp";

        private const int Year = 2026;
        private const int Month = 6;
        private const int StartDay = 1;
        private const int EndDay = 2;

        private const int DefaultId = 1;
        private const int DefaultCompanyId = 10;
        private const int ExpectedUserId = 42;
        private const int AltEventId = 5;
        private const int AltCompanyId = 99;
        private const int Company1Id = 1;
        private const int Company2Id = 2;

        private const int CountZero = 0;
        private const int CountTwo = 2;

        private Mock<HttpMessageHandler> _mockHandler = null!;
        private HttpClient _httpClient = null!;
        private CollaboratorsService collaboratorsService = null!;

        // Used to capture data sent in POST requests
        private CollaboratorDto? _lastPostedDto;
        private string? _lastPostUri;

        public CollaboratorsServiceTests()
        {
            _lastPostedDto = null;
            _lastPostUri = null;

            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            // Intercept POST requests to capture the URI and the CollaboratorDto payload
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
                        _lastPostedDto = System.Text.Json.JsonSerializer.Deserialize<CollaboratorDto>(
                            json,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("https://localhost/api/")
            };

            collaboratorsService = new CollaboratorsService(_httpClient);
        }

        private static Event MakeEvent()
        {
            return new Event(
                EventPhotoPath,
                EventTitle,
                EventDescription,
                new DateTime(Year, Month, StartDay),
                new DateTime(Year, Month, EndDay),
                EventLocation,
                DefaultId)
            { Id = DefaultId };
        }

        private static Company MakeCompany()
        {
            return new Company(DefaultCompanyName, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DefaultCompanyId);
        }

        private void SetupGetCollaboratorsResponse(int companyId, List<CompanyDto> dtos)
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains($"collaborators/{companyId}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(dtos)
                });
        }

        [Fact]
        public async Task AddCollaborator_ValidInputs_SendsPostRequest()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = MakeCompany();

            await collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, DefaultId);

            Assert.NotNull(_lastPostedDto);
            Assert.Contains("collaborators", _lastPostUri);
        }

        [Fact]
        public async Task AddCollaborator_ValidInputs_SendsCorrectEventId()
        {
            Event eventToCollaborateOn = MakeEvent();
            eventToCollaborateOn.Id = AltEventId;
            Company companyToAdd = MakeCompany();

            await collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, DefaultId);

            Assert.Equal(AltEventId, _lastPostedDto?.EventId);
        }

        [Fact]
        public async Task AddCollaborator_ValidInputs_SendsCorrectCompanyId()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = new Company(AltCompanyName, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, AltCompanyId);

            await collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, DefaultId);

            Assert.Equal(AltCompanyId, _lastPostedDto?.CompanyId);
        }

        [Fact]
        public async Task AddCollaborator_ValidInputs_SendsCorrectLoggedInUserIdInUri()
        {
            Event eventToCollaborateOn = MakeEvent();
            Company companyToAdd = MakeCompany();

            await collaboratorsService.AddCollaborator(eventToCollaborateOn, companyToAdd, ExpectedUserId);

            Assert.Contains($"loggedInUserID={ExpectedUserId}", _lastPostUri);
        }

        [Fact]
        public async Task GetAllCollaborators_ApiReturnsTwoCompanies_ServiceReturnsTwoCompanies()
        {
            var dtos = new List<CompanyDto>
            {
                new CompanyDto { CompanyId = Company1Id, Name = Company1Name },
                new CompanyDto { CompanyId = Company2Id, Name = Company2Name }
            };
            SetupGetCollaboratorsResponse(DefaultId, dtos);

            List<Company> result = await collaboratorsService.GetAllCollaborators(DefaultId);

            Assert.Equal(CountTwo, result.Count);
        }

        [Fact]
        public async Task GetAllCollaborators_ApiReturnsEmptyList_ServiceReturnsEmptyList()
        {
            var dtos = new List<CompanyDto>();
            SetupGetCollaboratorsResponse(DefaultId, dtos);

            List<Company> result = await collaboratorsService.GetAllCollaborators(DefaultId);

            Assert.Equal(CountZero, result.Count);
        }

        [Fact]
        public async Task GetAllCollaborators_ApiReturnsOneCompany_ServiceReturnsCorrectCompanyName()
        {
            var dtos = new List<CompanyDto>
            {
                new CompanyDto { CompanyId = DefaultId, Name = SingleCompanyName }
            };
            SetupGetCollaboratorsResponse(DefaultId, dtos);

            List<Company> result = await collaboratorsService.GetAllCollaborators(DefaultId);

            Assert.Equal(SingleCompanyName, result[0].Name);
        }

        [Fact]
        public void DefaultConstructor_InitializesHttpClient()
        {
            // Act
            var service = new CollaboratorsService();

            // Assert
            var httpField = typeof(CollaboratorsService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(httpField);
            Assert.Equal(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }
    }
}