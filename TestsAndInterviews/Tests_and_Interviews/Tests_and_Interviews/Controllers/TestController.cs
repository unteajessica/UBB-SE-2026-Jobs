namespace Tests_and_Interviews.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// TestController class serves as the entry point for handling test-related operations in the application.
    /// It interacts with the TestService and TimerService to manage test attempts, retrieve available tests, and handle
    /// test expiration logic. The controller provides methods for starting a test, submitting a test, fetching available
    /// tests based on category, and managing expired tests. This design allows for a clear separation of concerns, where
    /// the controller focuses on orchestrating the flow of data and delegating business logic to the respective services.
    /// </summary>
    public class TestController
    {
        private readonly ITestService testService;
        private readonly ITimerService timerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestController"/> class.
        /// </summary>
        /// <param name="testService">An instance of the <see cref="TestService"/> class to handle test-related operations.</param>
        /// <param name="timerService">An instance of the <see cref="TimerService"/> class to manage test expiration logic.</param>
        public TestController(ITestService testService, ITimerService timerService)
        {
            this.testService = testService;
            this.timerService = timerService;
        }

        /// <summary>
        /// Starts a test attempt for a given user and test ID.
        /// This method delegates the logic to the TestService to create a new test attempt
        /// </summary>
        /// <param name="userId">The ID of the user who is starting the test.</param>
        /// <param name="testId">The ID of the test that the user is starting.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartTestAsync(int userId, int testId)
        {
            await this.testService.StartTestAsync(userId, testId);
        }

        /// <summary>
        /// Submits a test attempt for a given attempt ID.
        /// This method delegates the logic to the TestService to finalize the test attempt and calculate
        /// the score based on the user's answers.
        /// </summary>
        /// <param name="attemptId">The ID of the test attempt that is being submitted.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SubmitTestAsync(int attemptId)
        {
            await this.testService.SubmitTestAsync(attemptId);
        }

        /// <summary>
        /// Gets a list of available tests for a given category.
        /// </summary>
        /// <param name="category">The category for which to retrieve available tests.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<Test>> GetAvailableTestsAsync(string category)
        {
            var tests = new List<Test>();

            var next = await this.testService.GetNextAvailableTestAsync(category);
            if (next != null)
            {
                tests.Add(next);
            }

            return tests;
        }

        /// <summary>
        /// Removes expired test attempts based on the provided attempt ID.
        /// This method checks if the test attempt has expired. If it has, it calls the TimerService to expire the test attempt asynchronously.
        /// </summary>
        /// <param name="attemptId">The ID of the test attempt to check for expiration.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RemoveExpiredTestsAsync(int attemptId)
        {
            if (this.timerService.CheckExpiration(attemptId))
            {
                await this.timerService.ExpireTestAsync(attemptId);
            }
        }

        /// <summary>
        /// Replaces expired test attempts by retrieving a list of expired attempt IDs from the
        /// TimerService and expiring each test attempt asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ReplaceExpiredTestsAsync()
        {
            var expiredIds = this.timerService.GetExpiredAttemptIds();

            foreach (var attemptId in expiredIds)
            {
                await this.timerService.ExpireTestAsync(attemptId);
            }
        }
    }
}