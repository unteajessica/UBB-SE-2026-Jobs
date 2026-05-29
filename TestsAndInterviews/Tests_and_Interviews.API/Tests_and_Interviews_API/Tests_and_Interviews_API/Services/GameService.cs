using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using Microsoft.Data.SqlClient;
using Tests_and_Interviews_API.Repositories.Interfaces;
using Tests_and_Interviews_API.Services.Interfaces;
using Tests_and_Interviews_API.Models;

namespace Tests_and_Interviews_API.Services
{
    public class GameService : IGameService
    {
        private readonly ICompanyRepo repository;
        public SessionService SessionService;

        public GameService(ICompanyRepo repository)
        {
            this.repository = repository;
        }

        public Game LoadedGame()
        {
            var game = repository.GetGame();
            if (game == null)
            {
                throw new InvalidOperationException("No game is available from the repository.");
            }
            return game;
        }

        public int GetBuddyId()
        {
            return LoadedGame().Buddy.Id;
        }

        public void Save(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }
            repository.SaveGame(game);
        }

        public Game GetStoredGame()
        {
            return repository.GetGame() ?? new Game();
        }

        public bool IsPublished()
        {
            var game = repository.GetGame();
            return game != null && game.IsPublished;
        }

        public string ShowCoworker()
        {
            return LoadedGame().Buddy.Introduction;
        }

        public string ShowScenarioText(int number)
        {
            var game = LoadedGame();
            if (number < 0 || number >= game.Scenarios.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Scenario index is out of bounds.");
            }

            return game.Scenarios[number].Description;
        }

        public List<string> ShowChoices(int number)
        {
            var game = LoadedGame();
            if (number < 0 || number >= game.Scenarios.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(number));
            }
            return game.Scenarios[number].GetAdviceTexts();
        }

        public string ChoiceMade(int numberScenario, int numberAdvice)
        {
            var game = LoadedGame();
            if (numberScenario < 0 || numberScenario >= game.Scenarios.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(numberScenario));
            }
            return game.Scenarios[numberScenario].SelectChoice(numberAdvice);
        }

        public string ShowConclusion()
        {
            return LoadedGame().Conclusion;
        }

        public Game CreateGameFromInput(
            int buddyId,
            string buddyName,
            string buddyIntroduction,
            IReadOnlyList<(string scenarioText, IReadOnlyList<(string advice, string feedback)> choices)> scenarios,
            string conclusion,
            bool publish = true)
        {
            var buddy = new Buddy(buddyId, buddyName, buddyIntroduction);

            var builtScenarios = scenarios
                .Select(scenarioData =>
                {
                    var scenario = new Scenario(scenarioData.scenarioText);
                    foreach (var (advice, feedback) in scenarioData.choices)
                    {
                        scenario.AddChoice(new AdviceChoice(advice, feedback));
                    }

                    return scenario;
                })
                .ToList();

            return new Game(buddy, builtScenarios, conclusion, publish);
        }

        public void PublishGame(Game existingGame)
        {
            existingGame.Publish();
        }

        public void UnpublishGame(Game existingGame)
        {
            existingGame.Unpublish();
        }
    }
}