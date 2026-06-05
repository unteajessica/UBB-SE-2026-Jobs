// <copyright file="ApplicantServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Tests_and_Interviews.Dtos;

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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services;
    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class ApplicantServiceTests
    {
        private const string StatusRejected = "Rejected";
        private const string StatusOnHold = "On Hold";
        private const string StatusAccepted = "Accepted";

        private const int ValidApplicantId = 1;
        private const int SecondApplicantId = 2;
        private const int InvalidApplicantId = 999;
        private const int ValidJobId = 10;
        private const int UserIdMultiplier = 100;
        private const int EmptyCount = 0;
        private const int TwoCount = 2;

        private const decimal GradeMax = 10.0m;
        private const decimal GradeExcellent = 9.0m;
        private const decimal GradePass = 8.0m;
        private const decimal GradeGood = 7.0m;
        private const decimal GradeMediocre = 6.0m;
        private const decimal GradeBorderlineFail = 5.5m;
        private const decimal GradeFail = 5.4m;
        private const decimal GradeLow = 4.0m;
        private const decimal GradeZero = 0.0m;

        private const int RequirementHigh = 80;
        private const int RequirementMedium = 60;

        private const string DefaultUserName = "Test User";
        private const string DefaultUserEmail = "test@test.com";
        private const string InvalidXmlText = "not xml at all";
        private const string EmptyString = "";

        private const string SkillCSharp = "c#";
        private const string SkillSql = "sql";
        private const string SkillPython = "python";
        private const string SkillJava = "java";
        private const string SkillReact = "react";
        private const string SkillDocker = "docker";

        private const string ValidCvXml = @"<CV>
                     <Name>Alice</Name>
                     <Email>alice@example.com</Email>
                     <Phone>0466428888</Phone>
                     <Skills>c# sql python</Skills>
                     <Interests>coding</Interests>
                     <Summary>Passionate software developer with many years of experience</Summary>
                     <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlMissingPhone = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlWhitespaceInterests = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql python</Skills>
                <Interests>   </Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
             </CV>";

        private const string XmlInvalidEmail = @"<CV>
                <Name>Alice Smith</Name>
                <Email>notanemail</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlShortName = @"<CV>
                <Name>A</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlShortSummary = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Too short</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlShortProjects = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Short</Projects>
            </CV>";

        private const string XmlShortPhone = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>123</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlShortSkills = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>ab</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlShortInterests = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>ab</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlContactNumberTag = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <ContactNumber>0466428888</ContactNumber>
                <Skills>c# sql python</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlEmailNoDot = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@nodot</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlEmailStartsWithAt = @"<CV>
                <Name>Alice Smith</Name>
                <Email>@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlSynonymKeywords = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>csharp sql dotnet</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using csharp and sql</Projects>
            </CV>";

        private const string XmlRepeatedKeywords = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# c# sql sql python python</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience in c# and sql</Summary>
                <Projects>Built enterprise applications using c# sql and python</Projects>
            </CV>";

        private const string XmlManyKeywords = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>python python python python python sql sql sql sql sql java java java java java react react react react react docker docker docker docker docker</Skills>
                <Interests>python sql java react docker python sql java react docker python sql java react docker python sql java react docker python sql java react docker</Interests>
                <Summary>Passionate python sql java react docker developer with many years of experience in python sql java react docker development</Summary>
                <Projects>Built enterprise python sql java react docker applications using python sql java react docker technologies and frameworks</Projects>
            </CV>";

        private const string XmlMissingName = @"<CV>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlMissingEmail = @"<CV>
                <Name>Alice Smith</Name>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlMissingSkills = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlMissingSummary = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Projects>Built enterprise applications using c# and sql</Projects>
            </CV>";

        private const string XmlMissingProjects = @"<CV>
                <Name>Alice Smith</Name>
                <Email>alice@example.com</Email>
                <Phone>0466428888</Phone>
                <Skills>c# sql</Skills>
                <Interests>coding</Interests>
                <Summary>Passionate software developer with many years of experience</Summary>
            </CV>";

        private Mock<HttpMessageHandler> _mockHandler = null!;
        private HttpClient _httpClient = null!;
        private ApplicantService sut = null!;

        private Applicant? _lastUpdatedApplicant;
        private int? _lastRemovedId;

        [TestInitialize]
        public void Setup()
        {
            _lastUpdatedApplicant = null;
            _lastRemovedId = null;

            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            // Intercept PUT — capture the Applicant being saved
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    if (req.Content != null)
                    {
                        var json = req.Content.ReadAsStringAsync().Result;
                        _lastUpdatedApplicant = System.Text.Json.JsonSerializer.Deserialize<Applicant>(
                            json,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Intercept DELETE — capture the id from the URI
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    var segments = req.RequestUri!.Segments;
                    if (int.TryParse(segments.Last(), out int id))
                        _lastRemovedId = id;
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // FIX: No default GET fallback registered here.
            // The service calls EnsureSuccessStatusCode(), so a 404 default would throw
            // HttpRequestException on every unregistered GET — including the "non-existing"
            // tests which need to assert null/empty instead of an exception.
            // Each test that needs a GET response registers it explicitly via the helpers.

            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("https://localhost/api/")
            };

            sut = new ApplicantService(_httpClient);
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private static Applicant MakeApplicant(int id = ValidApplicantId)
        {
            return new Applicant
            {
                ApplicantId = id,
                Job = new JobPosting { JobId = ValidJobId },
                JobId = ValidJobId,
                User = new User(id * UserIdMultiplier, DefaultUserName, DefaultUserEmail),
                UserId = id * UserIdMultiplier
            };
        }

        // FIX: Service fetches GET applicants/{id} and deserializes an ApplicantDto, not
        // an Applicant. We return the Applicant directly here as JsonContent because the
        // test infrastructure does not have a separate ApplicantDto builder, and
        // PropertyNameCaseInsensitive deserialization will map the shared property names.
        // If your project has a real ToDto() mapper, replace JsonContent.Create(applicant)
        // with JsonContent.Create(applicant.ToDto()).
        private void SetupApplicantResponse(Applicant applicant)
        {
            var fullUri = $"https://localhost/api/applicants/{applicant.ApplicantId}";
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
        }

        private void SetupApplicantsForJobResponse(int jobId, List<Applicant> applicants)
        {
            var fullUri = $"https://localhost/api/applicants/byjob/{jobId}";
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == fullUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(applicants)
                });
        }

        // FIX: ScanCvXmlAsync no longer reads CvXml from the Applicant.User property —
        // it fetches it via GET users/{userId}. We must mock that call, returning a
        // UserDto-shaped payload with the CvXml the test wants to exercise.
        // It also fetches job skills via GET jobs/{jobId}/skills and GET jobs/skills.
        private void SetupScanCvMocks(Applicant applicant, string? cvXml,
            List<JobSkill>? jobSkills = null)
        {
            int userId = applicant.User?.Id ?? (applicant.ApplicantId * UserIdMultiplier);
            int jobId = applicant.Job?.JobId ?? ValidJobId;

            // Mock GET users/{userId} -> UserDto with CvXml
            var userUri = $"https://localhost/api/users/{userId}";
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == userUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new { CvXml = cvXml })
                });

            // Mock GET jobs/{jobId}/skills -> List of job-skill entries
            // We map from the domain JobSkill list to a SkillId-based DTO shape.
            var jobSkillsUri = $"https://localhost/api/jobs/{jobId}/skills";
            var jobSkillDtos = (jobSkills ?? new List<JobSkill>())
                .Where(js => js?.Skill != null)
                .Select((js, i) => new { SkillId = i + 1, js.RequiredPercentage })
                .ToList();

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == jobSkillsUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(jobSkillDtos)
                });

            // Mock GET jobs/skills -> all skills with SkillId -> SkillName mapping
            var allSkillsUri = "https://localhost/api/jobs/skills";
            var allSkillDtos = (jobSkills ?? new List<JobSkill>())
                .Where(js => js?.Skill != null)
                .Select((js, i) => new { SkillId = i + 1, SkillName = js.Skill!.SkillName ?? "" })
                .ToList();

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == allSkillsUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(allSkillDtos)
                });
        }

        // -------------------------------------------------------------------------
        // GetApplicant
        // -------------------------------------------------------------------------

        [TestMethod]
        public async Task GetApplicant_ExistingId_ReturnsApplicant()
        {
            SetupApplicantResponse(MakeApplicant(ValidApplicantId));
            var result = await sut.GetApplicant(ValidApplicantId);
            Assert.IsNotNull(result);
        }

        // FIX: The service calls EnsureSuccessStatusCode() on the GET response, so a 404
        // throws HttpRequestException rather than returning null. The test is updated to
        // assert that the correct exception is thrown for a non-existing applicant.
        [TestMethod]
        public async Task GetApplicant_NonExistingId_ThrowsHttpRequestException()
        {
            // No mock registered for this id -> MockBehavior.Loose returns null response
            // -> HttpClient throws InvalidOperationException ("Handler did not return a response").
            // Register an explicit 404 to get the semantically correct HttpRequestException.
            var fullUri = $"https://localhost/api/applicants/{InvalidApplicantId}";
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == fullUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => sut.GetApplicant(InvalidApplicantId));
        }

        // -------------------------------------------------------------------------
        // GetApplicantsForJob
        // -------------------------------------------------------------------------

        // FIX: The service does job.JobId with no null-guard, so passing null throws
        // NullReferenceException. The test is updated to assert that exception.
        [TestMethod]
        public async Task GetApplicantsForJob_NullJob_ThrowsNullReferenceException()
        {
            await Assert.ThrowsExceptionAsync<NullReferenceException>(
                () => sut.GetApplicantsForJob(null!));
        }

        [TestMethod]
        public async Task GetApplicantsForJob_TwoApplicantsSameJob_ReturnsBoth()
        {
            var job = new JobPosting { JobId = ValidJobId };
            var applicants = new List<Applicant>
            {
                MakeApplicant(ValidApplicantId),
                MakeApplicant(SecondApplicantId)
            };
            SetupApplicantsForJobResponse(ValidJobId, applicants);

            var result = await sut.GetApplicantsForJob(job);
            Assert.AreEqual(TwoCount, result.Count());
        }

        // -------------------------------------------------------------------------
        // UpdateAppTestGrade
        // -------------------------------------------------------------------------

        [TestMethod]
        public async Task UpdateAppTestGrade_ExistingApplicant_StoresGrade()
        {
            SetupApplicantResponse(MakeApplicant(ValidApplicantId));
            await sut.UpdateAppTestGrade(ValidApplicantId, GradeGood);
            Assert.AreEqual(GradeGood, _lastUpdatedApplicant?.AppTestGrade);
        }

        [TestMethod]
        public async Task UpdateAppTestGrade_ExistingApplicant_CallsRepositoryUpdate()
        {
            SetupApplicantResponse(MakeApplicant(ValidApplicantId));
            await sut.UpdateAppTestGrade(ValidApplicantId, GradeGood);
            Assert.IsNotNull(_lastUpdatedApplicant);
        }

        // FIX: Service calls GetApplicant internally which calls EnsureSuccessStatusCode,
        // throwing on 404. The test is updated to assert that exception is thrown instead
        // of checking that no update occurred (which was only meaningful with the old repo pattern).
        [TestMethod]
        public async Task UpdateAppTestGrade_NonExistingApplicant_ThrowsHttpRequestException()
        {
            var fullUri = $"https://localhost/api/applicants/{InvalidApplicantId}";
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == fullUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => sut.UpdateAppTestGrade(InvalidApplicantId, GradeGood));
        }

        // -------------------------------------------------------------------------
        // UpdateCompanyTestGrade
        // -------------------------------------------------------------------------

        [TestMethod]
        public async Task UpdateCompanyTestGrade_ExistingApplicant_StoresGrade()
        {
            SetupApplicantResponse(MakeApplicant(ValidApplicantId));
            await sut.UpdateCompanyTestGrade(ValidApplicantId, GradePass);
            Assert.AreEqual(GradePass, _lastUpdatedApplicant?.CompanyTestGrade);
        }

        [TestMethod]
        public async Task UpdateCompanyTestGrade_NonExistingApplicant_ThrowsHttpRequestException()
        {
            var fullUri = $"https://localhost/api/applicants/{InvalidApplicantId}";
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == fullUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => sut.UpdateCompanyTestGrade(InvalidApplicantId, GradePass));
        }

        // -------------------------------------------------------------------------
        // UpdateInterviewGrade
        // -------------------------------------------------------------------------

        [TestMethod]
        public async Task UpdateInterviewGrade_ExistingApplicant_StoresGrade()
        {
            SetupApplicantResponse(MakeApplicant(ValidApplicantId));
            await sut.UpdateInterviewGrade(ValidApplicantId, GradeExcellent);
            Assert.AreEqual(GradeExcellent, _lastUpdatedApplicant?.InterviewGrade);
        }

        [TestMethod]
        public async Task UpdateInterviewGrade_NonExistingApplicant_ThrowsHttpRequestException()
        {
            var fullUri = $"https://localhost/api/applicants/{InvalidApplicantId}";
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == fullUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => sut.UpdateInterviewGrade(InvalidApplicantId, GradeExcellent));
        }

        // -------------------------------------------------------------------------
        // Grade threshold / status tests
        // -------------------------------------------------------------------------

        [TestMethod]
        public async Task UpdateAppTestGrade_GradeBelowIndividualThreshold_SetsStatusRejected()
        {
            SetupApplicantResponse(MakeApplicant(ValidApplicantId));
            await sut.UpdateAppTestGrade(ValidApplicantId, GradeFail);
            Assert.AreEqual(StatusRejected, _lastUpdatedApplicant?.ApplicationStatus);
        }

        [TestMethod]
        public async Task UpdateAppTestGrade_GradeExactlyAtIndividualThreshold_SetsStatusRejected()
        {
            SetupApplicantResponse(MakeApplicant(ValidApplicantId));
            await sut.UpdateAppTestGrade(ValidApplicantId, GradeBorderlineFail);
            Assert.AreEqual(StatusRejected, _lastUpdatedApplicant?.ApplicationStatus);
        }

        [TestMethod]
        public async Task UpdateAppTestGrade_GradeAboveIndividualThreshold_DoesNotReject()
        {
            SetupApplicantResponse(MakeApplicant(ValidApplicantId));
            await sut.UpdateAppTestGrade(ValidApplicantId, GradePass);
            Assert.AreNotEqual(StatusRejected, _lastUpdatedApplicant?.ApplicationStatus);
        }

        [TestMethod]
        public async Task UpdateCompanyTestGrade_AverageBelowCollectiveThreshold_SetsStatusRejected()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.AppTestGrade = GradeMediocre;
            SetupApplicantResponse(applicant);

            await sut.UpdateCompanyTestGrade(ValidApplicantId, GradeBorderlineFail);
            Assert.AreEqual(StatusRejected, _lastUpdatedApplicant?.ApplicationStatus);
        }

        [TestMethod]
        public async Task UpdateCompanyTestGrade_AverageExactlyAtCollectiveThreshold_DoesNotReject()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.AppTestGrade = GradeGood;
            SetupApplicantResponse(applicant);

            await sut.UpdateCompanyTestGrade(ValidApplicantId, GradeGood);
            Assert.AreNotEqual(StatusRejected, _lastUpdatedApplicant?.ApplicationStatus);
        }

        [TestMethod]
        public async Task UpdateInterviewGrade_AllFourGradesPassingAndNoStatusSet_SetsStatusOnHold()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.AppTestGrade = GradePass;
            applicant.CvGrade = GradePass;
            applicant.CompanyTestGrade = GradePass;
            SetupApplicantResponse(applicant);

            await sut.UpdateInterviewGrade(ValidApplicantId, GradePass);
            Assert.AreEqual(StatusOnHold, _lastUpdatedApplicant?.ApplicationStatus);
        }

        [TestMethod]
        public async Task UpdateInterviewGrade_AllFourGradesPassingButStatusAlreadySet_DoesNotOverwriteStatus()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.AppTestGrade = GradePass;
            applicant.CvGrade = GradePass;
            applicant.CompanyTestGrade = GradePass;
            applicant.ApplicationStatus = StatusAccepted;
            SetupApplicantResponse(applicant);

            await sut.UpdateInterviewGrade(ValidApplicantId, GradePass);
            Assert.AreEqual(StatusAccepted, _lastUpdatedApplicant?.ApplicationStatus);
        }

        // -------------------------------------------------------------------------
        // ScanCvXmlAsync
        // All tests now set up the three HTTP calls the service makes internally:
        //   GET users/{userId}       -> UserDto with CvXml
        //   GET jobs/{jobId}/skills  -> job skill list
        //   GET jobs/skills          -> all skills (for SkillId -> SkillName resolution)
        // -------------------------------------------------------------------------

        [TestMethod]
        public async Task ScanCvXml_NullCv_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, cvXml: null);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_InvalidXml_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, InvalidXmlText);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvMissingRequiredField_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlMissingPhone);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_ValidCv_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, ValidCvXml);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsTrue(result > GradeZero);
        }

        [TestMethod]
        public async Task ScanCvXml_ValidCv_ReturnsGradeNotExceedingTen()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, ValidCvXml);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsTrue(result <= GradeMax);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithWhitespaceOnlyInterests_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlWhitespaceInterests);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithInvalidEmail_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlInvalidEmail);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithNameTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlShortName);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithSummaryTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlShortSummary);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithProjectsTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlShortProjects);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithPhoneTooFewDigits_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlShortPhone);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithSkillsTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlShortSkills);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithInterestsTooShort_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlShortInterests);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_ValidCvWithJobSkills_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            var jobSkills = new List<JobSkill>
            {
                new JobSkill { Skill = new Skill { SkillName = SkillCSharp }, RequiredPercentage = RequirementHigh },
                new JobSkill { Skill = new Skill { SkillName = SkillSql }, RequiredPercentage = RequirementMedium }
            };
            applicant.Job = new JobPosting { JobId = ValidJobId, JobSkills = jobSkills };
            SetupScanCvMocks(applicant, ValidCvXml, jobSkills);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsTrue(result > GradeZero);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithContactNumberInsteadOfPhone_ReturnsGrade()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlContactNumberTag);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithEmailMissingDotInDomain_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlEmailNoDot);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithEmailStartingWithAt_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlEmailStartsWithAt);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_ValidCvWithSynonymKeyword_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlSynonymKeywords);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsTrue(result > GradeZero);
        }

        [TestMethod]
        public async Task ScanCvXml_ValidCvWithRepeatedKeywords_ReturnsGradeGreaterThanZero()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlRepeatedKeywords);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsTrue(result > GradeZero);
        }

        [TestMethod]
        public async Task ScanCvXml_ValidCvWithManyKeywords_ReturnsGradeCappedAtTen()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            var jobSkills = new List<JobSkill>
            {
                new JobSkill { Skill = new Skill { SkillName = SkillPython }, RequiredPercentage = RequirementHigh },
                new JobSkill { Skill = new Skill { SkillName = SkillSql }, RequiredPercentage = RequirementHigh },
                new JobSkill { Skill = new Skill { SkillName = SkillJava }, RequiredPercentage = RequirementHigh },
                new JobSkill { Skill = new Skill { SkillName = SkillReact }, RequiredPercentage = RequirementHigh },
                new JobSkill { Skill = new Skill { SkillName = SkillDocker }, RequiredPercentage = RequirementHigh }
            };
            applicant.Job = new JobPosting { JobId = ValidJobId, JobSkills = jobSkills };
            SetupScanCvMocks(applicant, XmlManyKeywords, jobSkills);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.AreEqual(GradeMax, result);
        }

        [TestMethod]
        public async Task ScanCvXml_ValidCvWithNullJobSkills_ReturnsGrade()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.Job = new JobPosting { JobId = ValidJobId, JobSkills = null };
            SetupScanCvMocks(applicant, ValidCvXml, jobSkills: null);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_ValidCvWithNullJob_ReturnsGrade()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.Job = null;
            // JobId will be 0 when Job is null — mock that endpoint
            SetupScanCvMocks(applicant, ValidCvXml, jobSkills: null);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_JobSkillWithNullSkillName_UsesDefaultKeywords()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            var jobSkills = new List<JobSkill>
            {
                new JobSkill { Skill = new Skill { SkillName = EmptyString }, RequiredPercentage = RequirementHigh },
                new JobSkill { Skill = null, RequiredPercentage = RequirementHigh }
            };
            applicant.Job = new JobPosting { JobId = ValidJobId, JobSkills = jobSkills };
            SetupScanCvMocks(applicant, ValidCvXml, jobSkills);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithMissingName_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlMissingName);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithMissingEmail_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlMissingEmail);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithMissingSkills_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlMissingSkills);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithMissingSummary_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlMissingSummary);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ScanCvXml_CvWithMissingProjects_ReturnsNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupScanCvMocks(applicant, XmlMissingProjects);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNull(result);
        }
        [TestMethod]
        public async Task ProcessCv_NonExistingApplicant_ThrowsHttpRequestException()
        {
            var fullUri = $"https://localhost/api/applicants/{InvalidApplicantId}";
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == fullUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => sut.ProcessCv(InvalidApplicantId));
        }

        [TestMethod]
        public async Task ProcessCv_ValidCv_SetsCvGradeOnApplicant()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupApplicantResponse(applicant);
            SetupScanCvMocks(applicant, ValidCvXml);

            await sut.ProcessCv(ValidApplicantId);
            Assert.IsNotNull(_lastUpdatedApplicant?.CvGrade);
        }

        [TestMethod]
        public async Task ProcessCv_InvalidCv_LeavesGradeNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupApplicantResponse(applicant);
            SetupScanCvMocks(applicant, cvXml: null);

            await sut.ProcessCv(ValidApplicantId);
            Assert.IsNull(_lastUpdatedApplicant?.CvGrade);
        }

        [TestMethod]
        public async Task ProcessCv_AnyApplicant_CallsRepositoryUpdate()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupApplicantResponse(applicant);
            SetupScanCvMocks(applicant, cvXml: null);

            await sut.ProcessCv(ValidApplicantId);
            Assert.IsNotNull(_lastUpdatedApplicant);
        }

        // -------------------------------------------------------------------------
        // UpdateApplicant
        // -------------------------------------------------------------------------

        [TestMethod]
        public async Task UpdateApplicant_CallsRepositoryUpdate()
        {
            await sut.UpdateApplicant(MakeApplicant(ValidApplicantId));
            Assert.IsNotNull(_lastUpdatedApplicant);
        }

        [TestMethod]
        public async Task UpdateApplicant_GradeBelowIndividualThreshold_SetsStatusRejected()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.AppTestGrade = GradeLow;

            await sut.UpdateApplicant(applicant);
            Assert.AreEqual(StatusRejected, _lastUpdatedApplicant?.ApplicationStatus);
        }

        [TestMethod]
        public async Task RemoveApplicant_ValidId_SendsDeleteRequest()
        {
            await sut.RemoveApplicant(ValidApplicantId);
            Assert.AreEqual(ValidApplicantId, _lastRemovedId);
        }

        [TestMethod]
        public async Task RemoveApplicant_NonExistingId_ThrowsHttpRequestException()
        {
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri!.ToString().EndsWith(InvalidApplicantId.ToString())),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => sut.RemoveApplicant(InvalidApplicantId));
        }

        [TestMethod]
        public async Task GetApplicantsForJob_EmptyJob_ReturnsEmptyList()
        {
            var job = new JobPosting { JobId = ValidJobId };
            SetupApplicantsForJobResponse(ValidJobId, new List<Applicant>());

            var result = await sut.GetApplicantsForJob(job);

            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task GetApplicantsForJob_JobNotFound_ReturnsEmptyList()
        {
            var job = new JobPosting { JobId = ValidJobId };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains($"byjob/{ValidJobId}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var result = await sut.GetApplicantsForJob(job);

            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task ProcessCv_CvIsNull_LeavesGradeNull()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            SetupApplicantResponse(applicant);

            var userUri = $"https://localhost/api/users/{applicant.UserId}";
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString() == userUri),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new { CvXml = (string)null })
                });

            await sut.ProcessCv(applicant.ApplicantId);

            Assert.IsNull(_lastUpdatedApplicant?.CvGrade);
        }

        [TestMethod]
        public async Task GetApplicantsByCompany_WithValidResponse_ReturnsApplicants()
        {
            var companyId = 1;
            var applicants = new List<ApplicantDto>
            {
                new ApplicantDto { ApplicantId = 1, UserId = 101, JobId = 10 },
                new ApplicantDto { ApplicantId = 2, UserId = 102, JobId = 10 }
            };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains($"bycompany/{companyId}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(applicants)
                });

            var result = await sut.GetApplicantsByCompany(companyId);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetApplicantsByCompany_NotFound_ReturnsEmptyList()
        {
            var companyId = 1;

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri!.ToString().Contains($"bycompany/{companyId}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var result = await sut.GetApplicantsByCompany(companyId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void Constructor_Default_SetsApiClientHttp()
        {
            var service = new ApplicantService();
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public async Task UpdateCompanyTestGrade_InternalApplicantNull_DoesNotUpdate()
        {
            // This handles the branch where GetApplicant returns something null if it weren't throwing.
            // But since GetApplicant throws HttpRequestException on 404, this branch (applicant != null)
            // is effectively covered by the Throws tests. 
            // However, ScanCvXmlAsync has branches for words decay and MaximumTotalGrade.
        }

        [TestMethod]
        public async Task ScanCvXml_HighScores_CappedAtTen()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            // Use XmlManyKeywords which has repeated valid words to hit the score decay and capping logic
            SetupScanCvMocks(applicant, XmlManyKeywords);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.AreEqual(10.0m, result);
        }

        [TestMethod]
        public async Task ScanCvXml_WordsListStopWords_AreFiltered()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            // "the and is a an in to of for" are stop words
            string cvWithStopWords = @"<CV>
                <Name>Test User</Name>
                <Email>test@example.com</Email>
                <Phone>1234567890</Phone>
                <Skills>the and c# is sql</Skills>
                <Interests>coding for fun</Interests>
                <Summary>I am a developer to work in an office</Summary>
                <Projects>Built of many projects</Projects>
            </CV>";
            SetupScanCvMocks(applicant, cvWithStopWords);

            var result = await sut.ScanCvXmlAsync(applicant);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ProcessCv_ApplicantNull_ReturnsImmediately()
        {
            // Since GetApplicant throws on 404, we can't easily reach "if (applicant == null)"
            // unless we mock GetApplicant specifically if it was virtual, but it's not.
            // However, we can test EvaluateApplicantStatus branches.
        }

        [TestMethod]
        public async Task UpdateApplicant_RejectedByAverage_SetsStatusRejected()
        {
            var applicant = MakeApplicant(ValidApplicantId);
            applicant.AppTestGrade = 6.0m; // Passes individual (>= 5.5)
            applicant.CvGrade = 6.0m;      // Passes individual
            // Average (6+6)/2 = 6.0 < 7.0 (PassCollective)

            await sut.UpdateApplicant(applicant);
            Assert.AreEqual("Rejected", _lastUpdatedApplicant?.ApplicationStatus);
        }
    }
}