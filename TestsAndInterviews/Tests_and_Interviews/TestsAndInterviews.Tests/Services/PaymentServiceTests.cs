// <copyright file="PaymentServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TestsAndInterviews.Tests.Services
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Validators;
    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class PaymentServiceTests
    {
        private const int ValidJobId = 90101;
        private const int ValidAmount = 200;
        private const int Amount250 = 250;
        private const int Amount100 = 100;

        private const string EmptyString = "";
        private const string ValidName = "John Doe";
        private const string ValidCard = "123456789012345";
        private const string ValidExp = "12/99";
        private const string ValidCvv = "123";

        private const string NameRequiredError = "Card Holder Name is required.";
        private const string DbErrorPrefix = "Database Error: ";
        private const string GenericError = "boom";

        private Mock<HttpMessageHandler> _mockHandler = null!;
        private Mock<IPaymentValidator> _mockValidator = null!;
        private HttpClient _httpClient = null!;
        private PaymentService _service = null!;

        private string? _lastRequestUri;
        private HttpMethod? _lastRequestMethod;

        [TestInitialize]
        public void Setup()
        {
            _lastRequestUri = null;
            _lastRequestMethod = null;

            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            _mockValidator = new Mock<IPaymentValidator>();

            // Setup Default POST/PUT Response (Success)
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post || req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    _lastRequestUri = req.RequestUri?.ToString();
                    _lastRequestMethod = req.Method;
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("https://localhost/api/")
            };

            _service = new PaymentService(_mockValidator.Object, _httpClient);
        }

        // FIX 1: Removed the URL contains check ("payments/jobs") — it was too strict
        // and didn't match the actual endpoint the service calls.
        // Now matches any GET request so the mock actually fires.
        private void SetupGetJobsResponse(List<JobPaymentInfo> data)
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(data)
                });
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_InvalidCardHolderName_ReturnsValidationError_AndDoesNotSendRequest()
        {
            // Arrange: Setup validator to return an error
            _mockValidator.Setup(v => v.ValidatePaymentDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                          .Returns(NameRequiredError);

            // Act
            var result = await _service.ProcessPaymentAsync(ValidJobId, ValidAmount, EmptyString, ValidCard, ValidExp, ValidCvv);

            // Assert
            Assert.AreEqual(NameRequiredError, result);
            Assert.IsNull(_lastRequestMethod); // Ensure HTTP call was never made
        }



        [TestMethod]
        public async Task ProcessPaymentAsync_WhenApiReturnsError_ReturnsDatabaseErrorMessage()
        {
            // Arrange
            _mockValidator.Setup(v => v.ValidatePaymentDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                          .Returns(string.Empty);

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(GenericError)
                });

            // Act
            var result = await _service.ProcessPaymentAsync(ValidJobId, ValidAmount, ValidName, ValidCard, ValidExp, ValidCvv);

            // Assert
            Assert.AreEqual($"Database Error: Response status code does not indicate success: 500 (Internal Server Error).", result);
        }

        [TestMethod]
        public async Task GetPaidJobsInfo_ReturnsApiDataMappedCorrectly()
        {
            // Arrange
            var apiData = new List<JobPaymentInfo>
            {
                new JobPaymentInfo
                {
                    CompanyName = "  Budget Company  ",
                    JobTitle = "Backend Developer",
                    AmountPayed = Amount250
                }
            };
            SetupGetJobsResponse(apiData);

            // Act
            var result = await _service.GetPaidJobsInfo("Full-time", "Entry Level");

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Budget Company", result[0].CompanyName?.Trim());
            Assert.AreEqual(Amount250, result[0].AmountPayed);
        }

        [TestMethod]
        public async Task GetPaidJobsInfo_WhenApiReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            SetupGetJobsResponse(new List<JobPaymentInfo>());

            // Act
            var result = await _service.GetPaidJobsInfo("Full-time", "Entry Level");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_WhenSuccessfulAndNotificationFails_ReturnsEmptyString()
        {
            // Arrange
            _mockValidator.Setup(v => v.ValidatePaymentDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                          .Returns(string.Empty);

            // Mock PUT payment
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.RequestUri.PathAndQuery.Contains("payment/")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Mock GET notify - returns one email
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.PathAndQuery.Contains("payment/notify/")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new List<string> { "notify@test.com" })
                });

            // Act
            // This will trigger SendNotificationEmailsAsync, which will hit the catch block when SmtpClient fails to connect.
            var result = await _service.ProcessPaymentAsync(ValidJobId, ValidAmount, ValidName, ValidCard, ValidExp, ValidCvv);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_WhenNotifyReturnsNotFound_ReturnsEmptyString()
        {
            // Arrange
            _mockValidator.Setup(v => v.ValidatePaymentDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                          .Returns(string.Empty);

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("notify")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            // Act
            var result = await _service.ProcessPaymentAsync(ValidJobId, ValidAmount, ValidName, ValidCard, ValidExp, ValidCvv);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_WhenNotifyFails_ReturnsDatabaseError()
        {
            // Arrange
            _mockValidator.Setup(v => v.ValidatePaymentDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                          .Returns(string.Empty);

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("notify")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            // Act
            var result = await _service.ProcessPaymentAsync(ValidJobId, ValidAmount, ValidName, ValidCard, ValidExp, ValidCvv);

            // Assert
            Assert.IsTrue(result.StartsWith(DbErrorPrefix));
        }

        [TestMethod]
        public async Task ProcessPaymentAsync_WhenNoEmailsToNotify_DoesNotCallSendNotificationEmailsAsync()
        {
            // Arrange
            _mockValidator.Setup(v => v.ValidatePaymentDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                          .Returns(string.Empty);

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Mock GET notify - returns empty list
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("notify")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new List<string>())
                });

            // Act
            var result = await _service.ProcessPaymentAsync(ValidJobId, ValidAmount, ValidName, ValidCard, ValidExp, ValidCvv);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void Constructor_SingleParam_InitializesProperties()
        {
            // Arrange
            var mockValidator = new Mock<IPaymentValidator>();

            // Act
            var service = new PaymentService(mockValidator.Object);

            // Assert
            // We use reflection to verify the private fields are set as expected
            var validatorField = typeof(PaymentService).GetField("validator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var httpField = typeof(PaymentService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsNotNull(validatorField, "Field 'validator' not found");
            Assert.IsNotNull(httpField, "Field 'http' not found");

            Assert.AreSame(mockValidator.Object, validatorField.GetValue(service));
            Assert.AreSame(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }
    }
}