// <copyright file="CompanyServiceTests.cs" company="PlaceholderCompany">
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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services;
    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class CompanyServiceTests
    {
        private const string ValidCompanyName = "TestCompany";
        private const string ValidAbout = "We build software.";
        private const string ValidPfp = "image.png";
        private const string ValidLogo = "logo.png";
        private const string ValidLocation = "Bucharest";
        private const string ValidEmail = "test@test.com";

        private const string ShortTestName = "Test";
        private const string SpecificCompanyName = "Google";

        private const int ValidCompanyId = 1;
        private const int RemoveCompanyId = 5;

        private Mock<HttpMessageHandler> _mockHandler = null!;
        private HttpClient _httpClient = null!;
        private CompanyService service = null!;

        // Captured variables to verify outgoing requests
        private CompanyDto? _lastPostedDto;
        private string? _lastPostUri;
        private CompanyDto? _lastPutDto;
        private string? _lastPutUri;
        private string? _lastDeleteUri;

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
                        _lastPostedDto = System.Text.Json.JsonSerializer.Deserialize<CompanyDto>(
                            json,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

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
                        _lastPutDto = System.Text.Json.JsonSerializer.Deserialize<CompanyDto>(
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

            // Default fallback for GET (to avoid null ref exceptions if unmocked)
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

            service = new CompanyService(_httpClient);
        }

        private void SetupGetResponse(string expectedUriFragment, CompanyDto dto)
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains(expectedUriFragment)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(dto)
                });
        }

        [TestMethod]
        public async Task AddCompany_ValidCompany_SendsPostRequest()
        {
            await service.AddCompany(ValidCompanyName, ValidAbout, ValidPfp, ValidLogo, ValidLocation, ValidEmail);

            Assert.IsNotNull(_lastPostedDto);
            Assert.AreEqual(ValidCompanyName, _lastPostedDto.Name);
            Assert.AreEqual(ValidAbout, _lastPostedDto.AboutUs);
            Assert.IsTrue(_lastPostUri!.EndsWith("companies"));
        }

        [TestMethod]
        public async Task AddCompany_InvalidName_ThrowsException()
        {
            // Validation happens before HTTP request, so this will throw without hitting the mock
            await Assert.ThrowsExceptionAsync<Exception>(() =>
                service.AddCompany(string.Empty, string.Empty, string.Empty, ValidLogo, string.Empty, string.Empty));
        }

        [TestMethod]
        public async Task GetCompanyById_ExistingCompany_ReturnsCompany()
        {
            var dto = new CompanyDto
            {
                CompanyId = ValidCompanyId,
                Name = ShortTestName,
                CompanyLogoPath = ValidLogo
            };
            SetupGetResponse($"companies/{ValidCompanyId}", dto);

            Company? result = await service.GetCompanyById(ValidCompanyId);

            Assert.IsNotNull(result);
            Assert.AreEqual(ValidCompanyId, result.CompanyId);
            Assert.AreEqual(ShortTestName, result.Name);
        }

        [TestMethod]
        public async Task UpdateCompany_ValidCompany_SendsPutRequest()
        {
            Company company = new Company(ShortTestName, ValidAbout, ValidPfp, ValidLogo, ValidLocation, ValidEmail)
            {
                CompanyId = ValidCompanyId
            };

            await service.UpdateCompany(company);

            Assert.IsNotNull(_lastPutDto);
            Assert.AreEqual(ShortTestName, _lastPutDto.Name);
            Assert.IsTrue(_lastPutUri!.EndsWith($"companies/{ValidCompanyId}"));
        }

        [TestMethod]
        public async Task RemoveCompany_SendsDeleteRequest()
        {
            await service.RemoveCompany(RemoveCompanyId);

            Assert.IsNotNull(_lastDeleteUri);
            Assert.IsTrue(_lastDeleteUri.EndsWith($"companies/{RemoveCompanyId}"));
        }

        [TestMethod]
        public async Task GetCompanyByName_ReturnsCompanyFromApi()
        {
            var dto = new CompanyDto
            {
                CompanyId = ValidCompanyId,
                Name = SpecificCompanyName,
                CompanyLogoPath = ValidLogo
            };
            SetupGetResponse($"companies/byname/{SpecificCompanyName}", dto);

            Company? result = await service.GetCompanyByName(SpecificCompanyName);

            Assert.IsNotNull(result);
            Assert.AreEqual(SpecificCompanyName, result.Name);
            Assert.AreEqual(ValidCompanyId, result.CompanyId);
        }

        [TestMethod]
        public void DefaultConstructor_InitializesFields()
        {
            // Act
            var service = new CompanyService();

            // Assert
            var validatorField = typeof(CompanyService).GetField("companyValidator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var httpField = typeof(CompanyService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsNotNull(validatorField);
            Assert.IsNotNull(httpField);
            Assert.AreSame(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }

        [TestMethod]
        public void ParametrizedConstructor_WithNull_InitializesWithDefault()
        {
            // Act
            var service = new CompanyService(null!);

            // Assert
            var httpField = typeof(CompanyService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(httpField);
            Assert.AreSame(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }
    }
}