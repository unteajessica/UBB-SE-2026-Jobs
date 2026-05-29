namespace Tests_and_Interviews.Tests.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using Tests_and_Interviews.Controllers;
    using Tests_and_Interviews.Models.Core;
    using Tests_and_Interviews.Services.Interfaces;
    using Xunit;

    public class TestControllerTests
    {

        private static (
            Mock<ITestService> testService,
            Mock<ITimerService> timerService,
            TestController testController)
            CreateMocks()
        {
            var testService = new Mock<ITestService>(MockBehavior.Strict);
            var timerService = new Mock<ITimerService>(MockBehavior.Strict);
            var testController = new TestController(testService.Object, timerService.Object);
            return (testService, timerService, testController);
        }

        [Fact]
        public async Task StartTest_DelegatesTo_TestService()
        {
            var (testService, _, testController) = CreateMocks();

            testService
                .Setup(service => service.StartTestAsync(1, 10))
                .Returns(Task.CompletedTask);

            await testController.StartTestAsync(1, 10);

            testService.Verify(service => service.StartTestAsync(1, 10), Times.Once);
        }

        [Fact]
        public async Task SubmitTest_DelegatesTo_TestService()
        {
            var (testService, _, testController) = CreateMocks();

            testService
                .Setup(service => service.SubmitTestAsync(1))
                .Returns(Task.CompletedTask);

            await testController.SubmitTestAsync(1);

            testService.Verify(service => service.SubmitTestAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetAvailableTests_WhenNextTestExists_ReturnsListWithOneTest()
        {
            var (testService, _, testController) = CreateMocks();
            var test = new Test { Id = 1, Title = "Test" };

            testService
                .Setup(service => service.GetNextAvailableTestAsync("math"))
                .ReturnsAsync(test);

            var result = await testController.GetAvailableTestsAsync("math");

            Assert.Single(result);
        }

        [Fact]
        public async Task GetAvailableTests_WhenNextTestExists_ReturnsCorrectTest()
        {
            var (testService, _, testController) = CreateMocks();
            var test = new Test { Id = 1, Title = "Test" };

            testService
                .Setup(service => service.GetNextAvailableTestAsync("math"))
                .ReturnsAsync(test);

            var result = await testController.GetAvailableTestsAsync("math");

            Assert.Equal(test, result[0]);
        }

        [Fact]
        public async Task GetAvailableTests_WhenNoTestExists_ReturnsEmptyList()
        {
            var (testService, _, testController) = CreateMocks();

            testService
                .Setup(service => service.GetNextAvailableTestAsync("math"))
                .ReturnsAsync((Test?)null);

            var result = await testController.GetAvailableTestsAsync("math");

            Assert.Empty(result);
        }

        [Fact]
        public async Task RemoveExpiredTests_WhenAttemptIsExpired_CallsExpireTest()
        {
            var (_, timerService, testController) = CreateMocks();

            timerService
                .Setup(service => service.CheckExpiration(1))
                .Returns(true);
            timerService
                .Setup(service => service.ExpireTestAsync(1))
                .Returns(Task.CompletedTask);

            await testController.RemoveExpiredTestsAsync(1);

            timerService.Verify(service => service.ExpireTestAsync(1), Times.Once);
        }

        [Fact]
        public async Task RemoveExpiredTests_WhenAttemptIsNotExpired_DoesNotCallExpireTest()
        {
            var (_, timerService, testController) = CreateMocks();

            timerService
                .Setup(service => service.CheckExpiration(1))
                .Returns(false);

            await testController.RemoveExpiredTestsAsync(1);

            timerService.Verify(service => service.ExpireTestAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ReplaceExpiredTests_CallsExpireTest_ForEachExpiredId()
        {
            var (_, timerService, testController) = CreateMocks();
            var expiredIds = new List<int> { 1, 2, 3 };

            timerService
                .Setup(service => service.GetExpiredAttemptIds())
                .Returns(expiredIds);
            timerService
                .Setup(service => service.ExpireTestAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            await testController.ReplaceExpiredTestsAsync();

            timerService.Verify(service => service.ExpireTestAsync(It.IsAny<int>()), Times.Exactly(3));
        }

        [Fact]
        public async Task ReplaceExpiredTests_WhenNoExpiredIds_DoesNotCallExpireTest()
        {
            var (_, timerService, testController) = CreateMocks();

            timerService
                .Setup(service => service.GetExpiredAttemptIds())
                .Returns(new List<int>());

            await testController.ReplaceExpiredTestsAsync();

            timerService.Verify(service => service.ExpireTestAsync(It.IsAny<int>()), Times.Never);
        }
    }
}