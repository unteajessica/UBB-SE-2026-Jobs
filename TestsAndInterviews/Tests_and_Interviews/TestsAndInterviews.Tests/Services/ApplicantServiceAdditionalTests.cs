// <copyright file="ApplicantServiceAdditionalTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TestsAndInterviews.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services;
    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class ApplicantServiceAdditionalTests
    {
        private const int ValidApplicantId = 1;
        private const int ValidJobId = 10;
        private const int ValidUserId = 100;
        private const decimal GradeExcellent = 9.0m;
        private const decimal GradePass = 8.0m;
        private const decimal GradeMediocre = 6.0m;

        private Mock<HttpMessageHandler> _mockHandler = null!;
        private HttpClient _httpClient = null!;
        private ApplicantService _sut = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("https://localhost/api/")
            };

            _sut = new ApplicantService(_httpClient);
        }

        private static Applicant MakeApplicant(int id)
        {
            return new Applicant
            {
                ApplicantId = id,
                Job = new JobPosting { JobId = ValidJobId },
                JobId = ValidJobId,
                User = new User(id * 100, "Test User", "test@test.com"),
                UserId = id * 100
            };
        }

        [TestMethod]
        public async Task UpdateCompanyTestGrade_ExistingApplicant_CallsRepositoryUpdate()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            var fullUri = $"https://localhost/api/applicants/{ValidApplicantId}";

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == fullUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(applicant)
                });

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            await _sut.UpdateCompanyTestGrade(ValidApplicantId, GradeExcellent);

            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task UpdateInterviewGrade_ExistingApplicant_CallsRepositoryUpdate()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            var fullUri = $"https://localhost/api/applicants/{ValidApplicantId}";

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == fullUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(applicant)
                });

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            await _sut.UpdateInterviewGrade(ValidApplicantId, GradeExcellent);

            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task UpdateAppTestGrade_ExistingApplicant_CallsRepositoryUpdate()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            var fullUri = $"https://localhost/api/applicants/{ValidApplicantId}";

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == fullUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(applicant)
                });

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            await _sut.UpdateAppTestGrade(ValidApplicantId, GradeExcellent);

            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task GetApplicantsByCompany_WithValidResponse_ReturnsApplicants()
        {
            var applicants = new List<Applicant>
            {
                MakeApplicant(1),
                MakeApplicant(2)
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains("bycompany")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(applicants)
                });

            var result = await _sut.GetApplicantsByCompany(1);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetApplicantsForJob_WithValidResponse_ReturnsApplicants()
        {
            var applicants = new List<Applicant>
            {
                MakeApplicant(1),
                MakeApplicant(2)
            };

            var job = new JobPosting { JobId = ValidJobId };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains("byjob")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(applicants)
                });

            var result = await _sut.GetApplicantsForJob(job);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task GetApplicant_ServerError_ThrowsException()
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains($"applicants/{ValidApplicantId}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            await _sut.GetApplicant(ValidApplicantId);
        }
    }
}
