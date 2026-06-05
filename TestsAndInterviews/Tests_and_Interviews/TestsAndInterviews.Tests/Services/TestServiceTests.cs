namespace PussyCats.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Models.Enums;
    using Tests_and_Interviews.Services;
    using Tests_and_Interviews.Services.Interfaces;
    using Tests_and_Interviews.Dtos; // Added
    using Tests_and_Interviews.Mappers; // Added
    using Xunit;

    public class TestServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<IGradingService> mockGradingService;
        private readonly Mock<ITimerService> mockTimerService;
        private readonly Mock<IAttemptValidationService> mockValidationService;
        private readonly Mock<IDataProcessingService> mockDataProcessingService;

        private TestAttemptDto? _lastPostedDto;
        private TestAttemptDto? _lastPutDto;
        private int _postCallCount;
        private int _putCallCount;

        public TestServiceTests()
        {
            this._mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            this.mockGradingService = new Mock<IGradingService>();
            this.mockTimerService = new Mock<ITimerService>();
            this.mockValidationService = new Mock<IAttemptValidationService>();
            this.mockDataProcessingService = new Mock<IDataProcessingService>();

            this._postCallCount = 0;
            this._putCallCount = 0;

            // Intercept POST
            this._mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    this._postCallCount++;
                    if (req.Content != null)
                    {
                        var json = req.Content.ReadAsStringAsync().Result;
                        this._lastPostedDto = System.Text.Json.JsonSerializer.Deserialize<TestAttemptDto>(
                            json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                })
                // StartTestAsync NEEDS the returned object to get the ID
                .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = JsonContent.Create(new TestAttempt { Id = 99 }.ToDto())
                });

            // Intercept PUT
            this._mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    this._putCallCount++;
                    if (req.Content != null)
                    {
                        var json = req.Content.ReadAsStringAsync().Result;
                        this._lastPutDto = System.Text.Json.JsonSerializer.Deserialize<TestAttemptDto>(
                            json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            this._httpClient = new HttpClient(this._mockHandler.Object) { BaseAddress = new Uri("https://localhost/api/") };
        }

        private TestService MakeTestService() => new TestService(
            mockGradingService.Object, mockTimerService.Object,
            mockValidationService.Object, mockDataProcessingService.Object, _httpClient);

        private void SetupGetRequest<T>(string uriPart, T responseData)
        {
            this._mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get && req.RequestUri!.ToString().Contains(uriPart)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(responseData) });
        }

        [Fact]
        public async Task StartTestAsync_WhenCalled_SetsStatusToInProgress()
        {
            var testService = this.MakeTestService();
            await testService.StartTestAsync(1, 1);

            Assert.Equal(1, this._postCallCount);
            Assert.Equal(TestStatus.IN_PROGRESS.ToString(), this._lastPostedDto!.Status);
            // Verify Timer was started with the ID returned by the POST mock (99)
            this.mockTimerService.Verify(t => t.StartTimer(99), Times.Once);
        }

        [Fact]
        public async Task SubmitTestAsync_WhenCalled_SendsPutRequest()
        {
            // SubmitTestAsync does TWO gets. We must mock both.
            var attempt = new TestAttempt { Id = 1, TestId = 10 }.ToDto();
            var answers = new List<AnswerDto>();

            SetupGetRequest("testattempts/1", attempt);
            SetupGetRequest("answers/byattempt/1", answers);

            var testService = this.MakeTestService();
            await testService.SubmitTestAsync(1);

            Assert.Equal(1, this._putCallCount);
            Assert.NotNull(this._lastPutDto);
        }

        [Fact]
        public async Task SubmitTestAsync_WhenAnswerHasSingleChoice_GradesSingleChoice()
        {
            // 1. Arrange
            var attemptId = 1;
            var attemptDto = new TestAttempt { Id = attemptId }.ToDto();

            // Ensure the QuestionTypeString matches the Enum member name exactly
            var question = new Question
            {
                QuestionTypeString = nameof(QuestionType.SINGLE_CHOICE)
            };

            var answers = new List<AnswerDto>
    {
        new Answer
        {
            Value = "1",
            Question = question
        }.ToDto()
    };

            SetupGetRequest($"testattempts/{attemptId}", attemptDto);
            SetupGetRequest($"answers/byattempt/{attemptId}", answers);

            var testService = this.MakeTestService();

            // 2. Act
            await testService.SubmitTestAsync(attemptId);

            // 3. Assert
            // Using It.IsAny ensures that even if the mapper created a NEW instance, 
            // Moq will still see that the method was called.
            this.mockGradingService.Verify(
                g => g.GradeSingleChoice(It.IsAny<Question>(), It.IsAny<Answer>()),
                Times.Once);
        }

        [Fact]
        public async Task GetNextAvailableTestAsync_WhenTestsExist_ReturnsFirstTest()
        {
            var testDtos = new List<TestDto>
            {
                new Test { Id = 1, Title = "C++ Basics" }.ToDto()
            };

            SetupGetRequest("tests/bycategory/Programming", testDtos);
            var testService = this.MakeTestService();

            var result = await testService.GetNextAvailableTestAsync("Programming");

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task SubmitAttemptAsync_WhenCalled_ReturnsScore()
        {
            // Arrange
            int userId = 1;
            int testId = 10;
            int attemptId = 99;
            float expectedScore = 85.5f;

            var attemptDto = new TestAttempt { Id = attemptId }.ToDto();
            var finalAttemptDto = new TestAttempt { Id = attemptId, Score = (decimal)expectedScore }.ToDto();
            var answers = new List<AnswerDto>
            {
                new AnswerDto { QuestionId = 1, Value = "Ans1" }
            };

            // Setup multi-step responses
            // 1. Initial GetAsync in SubmitAttemptAsync
            // 2. SubmitTestAsync calling GetAsync for attempt
            // 3. SubmitTestAsync calling GetAsync for answers
            // 4. Final GetAsync in SubmitAttemptAsync for result
            
            // We use SetupSequence for the repeating URL
            this._mockHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().Contains($"testattempts/byuser/{userId}/bytest/{testId}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(attemptDto) })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(finalAttemptDto) });

            SetupGetRequest($"testattempts/{attemptId}", attemptDto);
            SetupGetRequest($"answers/byattempt/{attemptId}", new List<AnswerDto>());

            var testService = this.MakeTestService();

            // Act
            var result = await testService.SubmitAttemptAsync(userId, testId, answers);

            // Assert
            Assert.Equal(expectedScore, result);
            this.mockDataProcessingService.Verify(d => d.ProcessFinalizedAttemptAsync(attemptId), Times.Once);
        }

        [Fact]
        public async Task FindTestsByCategoryAsync_WhenTestsExist_ReturnsList()
        {
            // Arrange
            string category = "Programming";
            var tests = new List<TestDto>
            {
                new Test { Id = 1, Title = "Test 1" }.ToDto(),
                new Test { Id = 2, Title = "Test 2" }.ToDto()
            };

            SetupGetRequest($"tests/bycategory/{category}", tests);
            var testService = this.MakeTestService();

            // Act
            var result = await testService.FindTestsByCategoryAsync(category);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task FindByIdAsync_WhenCalled_ReturnsTest()
        {
            // Arrange
            int testId = 123;
            var testDto = new Test { Id = testId, Title = "Specific Test" }.ToDto();

            SetupGetRequest($"tests/{testId}", testDto);
            var testService = this.MakeTestService();

            // Act
            var result = await testService.FindByIdAsync(testId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testId, result.Id);
        }

        [Fact]
        public void Constructor_FourParams_InitializesProperties()
        {
            // Arrange
            var grading = new Mock<IGradingService>();
            var timer = new Mock<ITimerService>();
            var validation = new Mock<IAttemptValidationService>();
            var dataProcessing = new Mock<IDataProcessingService>();

            // Act
            var service = new TestService(grading.Object, timer.Object, validation.Object, dataProcessing.Object);

            // Assert
            var gradingField = typeof(TestService).GetField("gradingService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var timerField = typeof(TestService).GetField("timerService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var validationField = typeof(TestService).GetField("validationService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dataProcessingField = typeof(TestService).GetField("dataProcessingService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var httpField = typeof(TestService).GetField("http", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.NotNull(gradingField);
            Assert.NotNull(timerField);
            Assert.NotNull(validationField);
            Assert.NotNull(dataProcessingField);
            Assert.NotNull(httpField);

            Assert.Same(grading.Object, gradingField.GetValue(service));
            Assert.Same(timer.Object, timerField.GetValue(service));
            Assert.Same(validation.Object, validationField.GetValue(service));
            Assert.Same(dataProcessing.Object, dataProcessingField.GetValue(service));
            Assert.Same(Tests_and_Interviews.Api.ApiClient.Http, httpField.GetValue(service));
        }
    }
}