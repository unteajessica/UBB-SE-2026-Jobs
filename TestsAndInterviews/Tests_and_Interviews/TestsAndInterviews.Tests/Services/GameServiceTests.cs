// <copyright file="GameServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PussyCats.Tests.Services
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
    using Tests_and_Interviews.Dtos; // Make sure Dto namespace is included
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services;
    using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class GameServiceTests
    {
        private const string DefaultBuddyName = "Buddy";
        private const string DefaultBuddyIntro = "Hello there";
        private const string AltBuddyIntro = "Hello";
        private const string DefaultScenarioText = "Scenario text";
        private const string AdviceOne = "Advice 1";
        private const string ReactionOne = "Reaction 1";
        private const string AdviceTwo = "Advice 2";
        private const string ReactionTwo = "Reaction 2";
        private const string DefaultConclusion = "Good ending";

        private const string ScenarioOneText = "Scenario 1";
        private const string AltAdviceOne = "Advice";
        private const string AltReactionOne = "Reaction";
        private const string ScenarioTwoText = "Scenario 2";
        private const string AltAdviceTwo = "Advice2";
        private const string AltReactionTwo = "Reaction2";

        private const int DefaultBuddyId = 1;
        private const int ValidIndex = 0;
        private const int ExpectedChoicesCount = 2;
        private const int InvalidIndexPositive = 5;
        private const int InvalidIndexNegative = -1;

        private Mock<HttpMessageHandler> _mockHandler = null!;
        private HttpClient _httpClient = null!;
        private GameService service = null!;
        private int companyId = 1;

        private Game? _mockedGame;
        private string? _mockedJson;
        private GameDto? _lastSavedDto; // Changed to GameDto because the service sends a DTO

        [TestInitialize]
        public void Setup()
        {
            _mockedGame = null;
            _mockedJson = null;
            _lastSavedDto = null;

            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            // Intercept GET requests and return the mocked game (Mapped to DTO as the service expects)
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(() =>
                {
                    if (_mockedJson != null)
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(_mockedJson)
                        };
                    }

                    return new HttpResponseMessage(_mockedGame != null ? HttpStatusCode.OK : HttpStatusCode.NotFound)
                    {
                        Content = _mockedGame != null ? JsonContent.Create(MapToDto(_mockedGame)) : null
                    };
                });

            // Intercept POST/PUT requests and capture the saved DTO payload
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post || req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    if (req.Content != null)
                    {
                        var json = req.Content.ReadAsStringAsync().Result;
                        _lastSavedDto = System.Text.Json.JsonSerializer.Deserialize<GameDto>(
                            json,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            _httpClient = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri("https://localhost/api/")
            };

            service = new GameService(companyId, _httpClient);
        }

        [TestMethod]
        public void Constructor_WithCompanyIdOnly_UsesDefaultHttpClient()
        {
            var gameService = new GameService(companyId);
            Assert.IsNotNull(gameService);
        }

        private Game CreateTestGame()
        {
            var buddy = new Buddy(DefaultBuddyId, DefaultBuddyName, DefaultBuddyIntro);

            // Scenario 1
            var scenario1 = new Scenario(DefaultScenarioText);
            scenario1.AddChoice(new AdviceChoice(AdviceOne, ReactionOne));
            scenario1.AddChoice(new AdviceChoice(AdviceTwo, ReactionTwo));
            scenario1.AddChoice(new AdviceChoice("Advice 3", "Reaction 3"));

            // Scenario 2
            var scenario2 = new Scenario(ScenarioTwoText);
            scenario2.AddChoice(new AdviceChoice(AltAdviceTwo, AltReactionTwo));
            scenario2.AddChoice(new AdviceChoice("Extra Advice", "Extra Reaction"));
            scenario2.AddChoice(new AdviceChoice("Advice 6", "Reaction 6"));

            var scenarios = new List<Scenario> { scenario1, scenario2 };

            return new Game(buddy, scenarios, DefaultConclusion, true);
        }

        // Helper method to simulate the backend mapping Game to GameDto
        private GameDto MapToDto(Game game)
        {
            var dto = new GameDto
            {
                AvatarId = game.Buddy.Id,
                BuddyName = game.Buddy.Name,
                BuddyDescription = game.Buddy.Introduction,
                FinalQuote = game.Conclusion,
                IsPublished = game.IsPublished
            };

            if (game.Scenarios.Count > 0)
            {
                dto.Scen1Text = game.Scenarios[0].Description;
                var texts = game.Scenarios[0].GetAdviceTexts();
                var reactions = game.Scenarios[0].GetAdviceReactions();
                if (texts.Count > 0) { dto.Scen1Answer1 = texts[0]; dto.Scen1Reaction1 = reactions[0]; }
                if (texts.Count > 1) { dto.Scen1Answer2 = texts[1]; dto.Scen1Reaction2 = reactions[1]; }
                if (texts.Count > 2) { dto.Scen1Answer3 = texts[2]; dto.Scen1Reaction3 = reactions[2]; }
            }

            if (game.Scenarios.Count > 1)
            {
                dto.Scen2Text = game.Scenarios[1].Description;
                var texts = game.Scenarios[1].GetAdviceTexts();
                var reactions = game.Scenarios[1].GetAdviceReactions();
                if (texts.Count > 0) { dto.Scen2Answer1 = texts[0]; dto.Scen2Reaction1 = reactions[0]; }
                if (texts.Count > 1) { dto.Scen2Answer2 = texts[1]; dto.Scen2Reaction2 = reactions[1]; }
                if (texts.Count > 2) { dto.Scen2Answer3 = texts[2]; dto.Scen2Reaction3 = reactions[2]; }
            }

            return dto;
        }

        [TestMethod]
        public async Task LoadedGame_ReturnsGame() // Changed to async Task
        {
            _mockedGame = CreateTestGame();
            var game = await service.LoadedGame(); // Awaited
            Assert.IsNotNull(game);
        }

        [TestMethod]
        public async Task GetBuddyId_ReturnsCorrectId()
        {
            _mockedGame = CreateTestGame();
            int id = await service.GetBuddyId();
            Assert.AreEqual(DefaultBuddyId, id);
        }

        [TestMethod]
        public async Task Save_Success()
        {
            _mockedGame = CreateTestGame();
            await service.Save(_mockedGame);

            Assert.IsNotNull(_lastSavedDto);
            Assert.AreEqual(_mockedGame.Buddy.Name, _lastSavedDto.BuddyName);
            Assert.AreEqual(_mockedGame.Scenarios[0].Description, _lastSavedDto.Scen1Text);
            Assert.AreEqual(_mockedGame.Scenarios[1].Description, _lastSavedDto.Scen2Text);
            Assert.AreEqual(_mockedGame.Scenarios[1].GetAdviceTexts()[2], _lastSavedDto.Scen2Answer3);

            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.RequestUri.ToString().Contains("/game")),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task LoadedGame_WithTwoScenariosAndThreeChoices_HitsAllMappingLines()
        {
            // Arrange
            _mockedGame = CreateTestGame(); // 2 scenarios, 3 choices each

            // Act
            var game = await service.LoadedGame();

            // Assert
            Assert.AreEqual(2, game.Scenarios.Count);
            Assert.AreEqual(3, game.GetScenario(1).AdviceChoices.Count);
            Assert.AreEqual(ScenarioTwoText, game.GetScenario(1).Description);
            Assert.AreEqual("Advice 6", game.GetScenario(1).AdviceChoices[2].Advice);
        }

        [TestMethod]
        public async Task Save_NullGame_ThrowsException() // Changed to async Task
        {
            Game game = null!;
            Func<Task> action = async () => await service.Save(game); // Using Func<Task>
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(action); // Awaited correctly
        }

        [TestMethod]
        public async Task LoadedGame_NullDto_ThrowsInvalidOperationException()
        {
            _mockedJson = "null";
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => service.LoadedGame());
        }

        [TestMethod]
        public async Task LoadedGame_EmptyScenarios_ReturnsGameWithNoScenarios()
        {
            var dto = new GameDto
            {
                AvatarId = 0,
                BuddyName = "Buddy",
                Scen1Text = "", // Empty
                Scen2Text = null // Null
            };
            _mockedJson = System.Text.Json.JsonSerializer.Serialize(dto);

            var game = await service.LoadedGame();
            Assert.AreEqual(0, game.Scenarios.Count);
        }

        [TestMethod]
        public async Task GetStoredGame_ApiReturnsNull_ReturnsNewGame()
        {
            _mockedJson = "null";
            var result = await service.GetStoredGame();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Scenarios.Count);
        }

        [TestMethod]
        public async Task GetStoredGame_NotFound_ReturnsNewGame()
        {
            _mockedGame = null;
            var result = await service.GetStoredGame();
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Scenarios.Count);
        }

        [TestMethod]
        public async Task GetStoredGame_WhenGameExists_ReturnsGame() // Changed to async Task
        {
            _mockedGame = CreateTestGame();
            var result = await service.GetStoredGame(); // Awaited
            Assert.IsNotNull(result);
            Assert.AreEqual(DefaultBuddyName, result.Buddy?.Name);
        }

        [TestMethod]
        public async Task IsPublished_ReturnsTrue()
        {
            _mockedGame = CreateTestGame();
            var result = await service.IsPublished();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ShowCoworker_ReturnsIntroduction()
        {
            _mockedGame = CreateTestGame();
            var intro = await service.ShowCoworker();
            Assert.AreEqual(DefaultBuddyIntro, intro);
        }

        [TestMethod]
        public async Task ShowScenarioText_ReturnsText()
        {
            _mockedGame = CreateTestGame();
            var text = await service.ShowScenarioText(ValidIndex);
            Assert.AreEqual(DefaultScenarioText, text);
        }

        [TestMethod]
        public async Task ShowScenarioText_InvalidIndex_ThrowsException() // Changed to async Task
        {
            _mockedGame = CreateTestGame();
            Func<Task> action = async () => await service.ShowScenarioText(InvalidIndexPositive); // Using Func<Task>
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(action);
        }

        [TestMethod]
        public async Task ShowChoices_ReturnsChoices()
        {
            _mockedGame = CreateTestGame();
            var choices = await service.ShowChoices(ValidIndex);


            Assert.AreEqual(3, choices.Count);
        }

        [TestMethod]
        public async Task ShowConclusion_ReturnsConclusion()
        {
            _mockedGame = CreateTestGame();
            var result = await service.ShowConclusion();
            Assert.AreEqual(DefaultConclusion, result);
        }

        [TestMethod]
        public void PublishGame_SetsGamePublished()
        {
            var game = CreateTestGame();
            game.Unpublish();
            service.PublishGame(game);
            Assert.IsTrue(game.IsPublished);
        }

        [TestMethod]
        public void UnpublishGame_SetsGameUnpublished()
        {
            var game = CreateTestGame();
            game.Publish();

            service.UnpublishGame(game);
            Assert.IsFalse(game.IsPublished);
        }

        [TestMethod]
        public async Task IsPublished_NoGame_ReturnsFalse()
        {
            _mockedGame = null;
            var result = await service.IsPublished();
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ShowChoices_InvalidIndex_ThrowsException() // Changed to async Task
        {
            _mockedGame = CreateTestGame();
            Func<Task> action = async () => await service.ShowChoices(InvalidIndexPositive); // Using Func<Task>
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(action);
        }

        [TestMethod]
        public async Task ChoiceMade_ReturnsReaction() // Changed to async Task
        {
            _mockedGame = CreateTestGame();
            var result = await service.ChoiceMade(ValidIndex, ValidIndex); // Awaited
            Assert.IsNotNull(result);
            Assert.AreEqual(ReactionOne, result);
        }

        [TestMethod]
        public void CreateGameFromInput_CreatesGameCorrectly()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                (ScenarioOneText, new List<(string, string)>
                {
                    (AltAdviceOne, AltReactionOne)
                }),

                (ScenarioTwoText, new List<(string, string)>
                {
                    (AltAdviceTwo, AltReactionTwo)
                })
            };

            var game = service.CreateGameFromInput(
                DefaultBuddyId,
                DefaultBuddyName,
                AltBuddyIntro,
                scenarios,
                DefaultConclusion,
                true);

            Assert.IsNotNull(game);
            Assert.AreEqual(DefaultConclusion, game.Conclusion);
        }

        [TestMethod]
        public async Task ChoiceMade_InvalidScenarioIndex_ThrowsException() // Changed to async Task
        {
            _mockedGame = CreateTestGame();
            Func<Task> action = async () => await service.ChoiceMade(InvalidIndexNegative, ValidIndex); // Using Func<Task>
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(action);
        }
    }
}
