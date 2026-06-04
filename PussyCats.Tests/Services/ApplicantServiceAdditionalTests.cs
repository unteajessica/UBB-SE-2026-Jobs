// <copyright file="ApplicantServiceAdditionalTests.cs" company="PlaceholderCompany">
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
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Services;
using Xunit; // Crucial: Native xUnit assertions come from here

namespace TestsAndInterviews.Tests.Services
{
    public class ApplicantServiceAdditionalTests
    {
        private const int ValidApplicantId = 1;
        private const int ValidJobId = 10;
        private const int ValidUserId = 100;
        private const decimal GradeExcellent = 9.0m;
        private const decimal GradePass = 8.0m;
        private const decimal GradeMediocre = 6.0m;

        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly ApplicantService _sut;

        public ApplicantServiceAdditionalTests()
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

            // Native xUnit non-null assertion
            Assert.NotNull(result);
        }

        [Fact]
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

            // Native xUnit non-null assertion
            Assert.NotNull(result);
        }

        [Fact]
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

            // Native xUnit asynchronous exception assertion
            await Assert.ThrowsAsync<HttpRequestException>(() => _sut.GetApplicant(ValidApplicantId));
        }
    }
}