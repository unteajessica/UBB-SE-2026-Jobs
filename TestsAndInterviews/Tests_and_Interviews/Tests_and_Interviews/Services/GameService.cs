// <copyright file="GameService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace Tests_and_Interviews.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Tests_and_Interviews.Api;
    using Tests_and_Interviews.Dtos;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services.Interfaces;

    public class GameService : IGameService
    {
        private readonly int companyId;
        private readonly HttpClient http;
        public SessionService SessionService;

        public GameService(int companyId)
        {
            this.companyId = companyId;
            this.http = ApiClient.Http;
        }

        public GameService(int companyId, HttpClient httpClient)
        {
            this.companyId = companyId;
            this.http = httpClient ?? ApiClient.Http;
        }

        private async Task<GameDto> FetchGameDtoAsync()
        {
            HttpResponseMessage response = await this.http.GetAsync($"companies/{this.companyId}/game");
            response.EnsureSuccessStatusCode();
            GameDto? dto = await response.Content.ReadFromJsonAsync<GameDto>();
            if (dto == null)
            {
                throw new InvalidOperationException("No game is available from the repository.");
            }
            return dto;
        }

        private static Game MapDtoToGame(GameDto dto)
        {
            var buddy = new Buddy(dto.AvatarId, dto.BuddyName, dto.BuddyDescription);
            var scenarios = new List<Scenario>();
            if (!string.IsNullOrEmpty(dto.Scen1Text))
            {
                var scenario1 = new Scenario(dto.Scen1Text);
                scenario1.AddChoice(new AdviceChoice(dto.Scen1Answer1, dto.Scen1Reaction1));
                scenario1.AddChoice(new AdviceChoice(dto.Scen1Answer2, dto.Scen1Reaction2));
                scenario1.AddChoice(new AdviceChoice(dto.Scen1Answer3, dto.Scen1Reaction3));
                scenarios.Add(scenario1);
            }
            if (!string.IsNullOrEmpty(dto.Scen2Text))
            {
                var scenario2 = new Scenario(dto.Scen2Text);
                scenario2.AddChoice(new AdviceChoice(dto.Scen2Answer1, dto.Scen2Reaction1));
                scenario2.AddChoice(new AdviceChoice(dto.Scen2Answer2, dto.Scen2Reaction2));
                scenario2.AddChoice(new AdviceChoice(dto.Scen2Answer3, dto.Scen2Reaction3));
                scenarios.Add(scenario2);
            }
            return new Game(buddy, scenarios, dto.FinalQuote, dto.IsPublished);
        }

        private static GameDto MapGameToDto(Game game, int companyId)
        {
            return new GameDto
            {
                AvatarId = game.Buddy.Id,
                BuddyName = game.Buddy.Name,
                BuddyDescription = game.Buddy.Introduction,
                FinalQuote = game.Conclusion,
                IsPublished = game.IsPublished,
                Scen1Text = game.GetScenario(0).Description,
                Scen1Answer1 = game.GetScenario(0).GetAdviceTexts()[0],
                Scen1Answer2 = game.GetScenario(0).GetAdviceTexts()[1],
                Scen1Answer3 = game.GetScenario(0).GetAdviceTexts()[2],
                Scen1Reaction1 = game.GetScenario(0).GetAdviceReactions()[0],
                Scen1Reaction2 = game.GetScenario(0).GetAdviceReactions()[1],
                Scen1Reaction3 = game.GetScenario(0).GetAdviceReactions()[2],
                Scen2Text = game.GetScenario(1).Description,
                Scen2Answer1 = game.GetScenario(1).GetAdviceTexts()[0],
                Scen2Answer2 = game.GetScenario(1).GetAdviceTexts()[1],
                Scen2Answer3 = game.GetScenario(1).GetAdviceTexts()[2],
                Scen2Reaction1 = game.GetScenario(1).GetAdviceReactions()[0],
                Scen2Reaction2 = game.GetScenario(1).GetAdviceReactions()[1],
                Scen2Reaction3 = game.GetScenario(1).GetAdviceReactions()[2],
            };
        }

        public async Task<Game> LoadedGame()
        {
            GameDto dto = await this.FetchGameDtoAsync();
            return MapDtoToGame(dto);
        }

        public async Task<int> GetBuddyId()
        {
            return (await this.LoadedGame()).Buddy.Id;
        }

        public async Task Save(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }
            GameDto dto = MapGameToDto(game, this.companyId);
            HttpResponseMessage response = await this.http.PutAsJsonAsync(
                $"companies/{this.companyId}/game",
                dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Game> GetStoredGame()
        {
            HttpResponseMessage response = await this.http.GetAsync($"companies/{this.companyId}/game");
            if (!response.IsSuccessStatusCode)
            {
                return new Game();
            }
            GameDto? dto = await response.Content.ReadFromJsonAsync<GameDto>();
            return dto == null ? new Game() : MapDtoToGame(dto);
        }

        public async Task<bool> IsPublished()
        {
            HttpResponseMessage response = await this.http.GetAsync($"companies/{this.companyId}/game");
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            GameDto? dto = await response.Content.ReadFromJsonAsync<GameDto>();
            return dto != null && dto.IsPublished;
        }

        public async Task<string> ShowCoworker()
        {
            return (await this.LoadedGame()).Buddy.Introduction;
        }

        public async Task<string> ShowScenarioText(int number)
        {
            var game = await this.LoadedGame();
            if (number < 0 || number >= game.Scenarios.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(number), "Scenario index is out of bounds.");
            }
            return game.Scenarios[number].Description;
        }

        public async Task<List<string>> ShowChoices(int number)
        {
            var game = await this.LoadedGame();
            if (number < 0 || number >= game.Scenarios.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(number));
            }
            return game.Scenarios[number].GetAdviceTexts();
        }

        public async Task<string> ChoiceMade(int numberScenario, int numberAdvice)
        {
            var game = await this.LoadedGame();
            if (numberScenario < 0 || numberScenario >= game.Scenarios.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(numberScenario));
            }
            return game.Scenarios[numberScenario].SelectChoice(numberAdvice);
        }

        public async Task<string> ShowConclusion()
        {
            return (await this.LoadedGame()).Conclusion;
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