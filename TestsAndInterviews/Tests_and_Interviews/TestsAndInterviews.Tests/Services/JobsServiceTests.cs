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

namespace TestsAndInterviews.Tests.Services
{
    public class JobsServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly JobsService _jobsService;

        public JobsServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/api/")
            };
            _jobsService = new JobsService(_httpClient);
        }

        [Fact]
        public void Constructor_WithoutArgs_DoesNotThrow()
        {
            // Act
            var service = new JobsService();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_DoesNotThrow()
        {
            // Act
            var service = new JobsService(null!);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task GetAllJobsAsync_WhenSuccessful_ReturnsJobs()
        {
            // Arrange
            var jobsDto = new List<JobPostingDto>
            {
                new JobPostingDto { JobId = 1, JobTitle = "Software Developer" },
                new JobPostingDto { JobId = 2, JobTitle = "QA Engineer" }
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().EndsWith("/jobs")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(jobsDto)
                });

            // Act
            var result = await _jobsService.GetAllJobsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Software Developer", result[0].JobTitle);
        }

        [Fact]
        public async Task GetAllJobsAsync_WhenNotFound_ReturnsEmptyList()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var result = await _jobsService.GetAllJobsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllSkillsAsync_WhenSuccessful_ReturnsSkills()
        {
            // Arrange
            var skillsDto = new List<SkillDto>
            {
                new SkillDto { SkillId = 1, SkillName = "C#" },
                new SkillDto { SkillId = 2, SkillName = "SQL" }
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().EndsWith("/jobs/skills")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(skillsDto)
                });

            // Act
            var result = await _jobsService.GetAllSkillsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("C#", result[0].SkillName);
        }

        [Fact]
        public async Task GetAllSkillsAsync_WhenNotFound_ReturnsEmptyList()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var result = await _jobsService.GetAllSkillsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddJob_WhenSuccessful_ReturnsJobId()
        {
            // Arrange
            var jobPosting = new JobPosting { JobId = 1, JobTitle = "New Job" };
            var skillLinks = new List<(int SkillId, int RequiredPercentage)> { (1, 80) };
            int expectedJobId = 123;

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri.ToString().EndsWith("/jobs")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(expectedJobId)
                });

            // Act
            var result = await _jobsService.AddJob(jobPosting, 10, skillLinks);

            // Assert
            Assert.Equal(expectedJobId, result);
        }

        [Fact]
        public async Task AddJob_WhenJobPostingIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jobsService.AddJob(null!, 1, new List<(int, int)>()));
        }

        [Fact]
        public async Task AddJob_WhenApiFails_ThrowsHttpRequestException()
        {
            // Arrange
            var jobPosting = new JobPosting { JobId = 1 };
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _jobsService.AddJob(jobPosting, 1, new List<(int, int)>()));
        }
    }
}
