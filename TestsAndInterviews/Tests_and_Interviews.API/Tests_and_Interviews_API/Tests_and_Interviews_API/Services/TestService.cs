// <copyright file="TestService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Tests_and_Interviews_API.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Mappers;
    using Tests_and_Interviews_API.Models.Core;
    using Tests_and_Interviews_API.Models.Enums;
    using Tests_and_Interviews_API.Repositories.Interfaces;
    using Tests_and_Interviews_API.Services.Interfaces;

    /// <summary>
    /// Provides operations for managing tests and test attempts.
    /// </summary>
    public class TestService : ITestService
    {
        private readonly ITestRepository testRepository;
        private readonly ITestAttemptRepository attemptRepository;
        private readonly IAnswerRepository answerRepository;
        private readonly IGradingService gradingService;
        private readonly ITimerService timerService;
        private readonly IAttemptValidationService validationService;
        private readonly IDataProcessingService dataProcessingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestService"/> class.
        /// </summary>
        public TestService(
            ITestRepository testRepository,
            ITestAttemptRepository attemptRepository,
            IAnswerRepository answerRepository,
            IGradingService gradingService,
            ITimerService timerService,
            IAttemptValidationService validationService,
            IDataProcessingService dataProcessingService)
        {
            this.testRepository = testRepository;
            this.attemptRepository = attemptRepository;
            this.answerRepository = answerRepository;
            this.gradingService = gradingService;
            this.timerService = timerService;
            this.validationService = validationService;
            this.dataProcessingService = dataProcessingService;
        }

        /// <summary>
        /// Asynchronously retrieves all test entities from the data store.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all test entities. The
        /// list will be empty if no tests are found.</returns>
        public async Task<List<Test>> GetAll()
        {
            return await this.testRepository.GetTestsASync();
        }

        /// <summary>
        /// Asynchronously retrieves a list of all available category names.
        /// </summary>
        /// <returns>A list of strings containing the names of all categories. The list is empty if no categories are found.</returns>
        public async Task<List<string>> GetCategories()
        {
            return await this.testRepository.GetAllCategories();
        }

        /// <summary>
        /// Asynchronously retrieves the test with the specified identifier, including its associated questions.
        /// </summary>
        /// <param name="id">The unique identifier of the test.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the test, or null if not found.</returns>
        public async Task<Test?> FindByIdAsync(int id)
        {
            return await this.testRepository.FindByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<List<Test>> FindTestsByCategoryAsync(string category)
        {
            return await this.testRepository.FindTestsByCategoryAsync(category);
        }

        /// <inheritdoc/>
        public async Task StartTestAsync(int userId, int testId)
        {
            await this.validationService.CheckExistingAttemptsAsync(userId, testId);

            var attempt = new TestAttempt
            {
                TestId = testId,
                ExternalUserId = userId,
                Status = TestStatus.IN_PROGRESS.ToString(),
                StartedAt = DateTime.UtcNow,
            };

            attempt.Start();

            await this.attemptRepository.SaveAsync(attempt);
            this.timerService.StartTimer(attempt.Id);
        }

        /// <inheritdoc/>
        public async Task SubmitTestAsync(int attemptId)
        {
            TestAttempt? attempt = await this.attemptRepository.FindByIdAsync(attemptId);

            if (attempt == null)
            {
                throw new InvalidOperationException($"Attempt {attemptId} not found.");
            }

            List<Answer> answers = await this.answerRepository.FindByAttemptAsync(attemptId);

            foreach (Answer answer in answers)
            {
                if (answer.Question == null)
                {
                    continue;
                }

                switch (answer.Question.Type)
                {
                    case QuestionType.SINGLE_CHOICE:
                        this.gradingService.GradeSingleChoice(answer.Question, answer);
                        break;
                    case QuestionType.MULTIPLE_CHOICE:
                        this.gradingService.GradeMultipleChoice(answer.Question, answer);
                        break;
                    case QuestionType.TEXT:
                        this.gradingService.GradeText(answer.Question, answer);
                        break;
                    case QuestionType.TRUE_FALSE:
                        this.gradingService.GradeTrueFalse(answer.Question, answer);
                        break;
                }
            }

            this.gradingService.CalculateFinalScore(attempt);
            attempt.Submit();

            await this.attemptRepository.UpdateAsync(attempt);
        }

        /// <inheritdoc/>
        public async Task<float> SubmitAttemptAsync(
            int userId, int testId, IEnumerable<AnswerDto> answers)
        {
            TestAttempt? attempt =
                await this.attemptRepository.FindByUserAndTestAsync(userId, testId);

            if (attempt == null)
            {
                return 0f;
            }

            foreach (AnswerDto answerDto in answers)
            {
                if (string.IsNullOrEmpty(answerDto.Value))
                {
                    continue;
                }

                var answer = new Answer
                {
                    AttemptId = attempt.Id,
                    QuestionId = answerDto.QuestionId,
                    Value = answerDto.Value,
                };

                await this.answerRepository.SaveAsync(answer);
            }

            await this.SubmitTestAsync(attempt.Id);
            await this.dataProcessingService.ProcessFinalizedAttemptAsync(attempt.Id);

            TestAttempt? finalAttempt =
                await this.attemptRepository.FindByUserAndTestAsync(userId, testId);

            return finalAttempt != null ? (float)(finalAttempt.Score ?? 0m) : 0f;
        }

        /// <summary>
        /// Asynchronously adds a new test entity to the data store.
        /// </summary>
        /// <param name="test">The test entity to add. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added test entity.</returns>
        public async Task<Test> AddTestASync(Test test)
        {
            test.CreatedAt = DateTime.UtcNow;
            await this.testRepository.AddAsync(test);

            return test;
        }

        /// <summary>
        /// Asynchronously updates an existing test with the specified identifier using the provided test data.
        /// </summary>
        /// <param name="id">The unique identifier of the test to update.</param>
        /// <param name="test">The test data to apply to the existing test. The <see cref="Test.Id"/> property is ignored and will be set
        /// to the specified <paramref name="id"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="Test"/>
        /// instance.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a test with the specified <paramref name="id"/> does not exist.</exception>
        public async Task<Test> UpdateTestAsync(int id, Test test)
        {
            Test? initialTest = await this.testRepository.FindByIdAsync(id);
            if (initialTest == null) {
                throw new KeyNotFoundException("Test to update not found.");
            }

            test.Id = id;
            await this.testRepository.UpdateAsync(test);

            return test;
        }

        /// <summary>
        /// Asynchronously deletes the test with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the test to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the test was
        /// successfully deleted; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if a test with the specified <paramref name="id"/> does not exist.</exception>
        public async Task<bool> DeleteTestAsync(int id)
        {
            Test? initialTest = await this.testRepository.FindByIdAsync(id);
            if (initialTest == null) {
                throw new KeyNotFoundException("Test to update not found.");
            }

            try {
                await this.testRepository.DeleteAsync(id);
                return true;
            } catch (Exception) {
                return false;
            }
        }
    }
}