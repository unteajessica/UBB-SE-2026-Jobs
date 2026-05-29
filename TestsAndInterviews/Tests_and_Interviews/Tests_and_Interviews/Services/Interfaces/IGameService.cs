using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Services.Interfaces
{
    public interface IGameService
    {
        Task<Game> LoadedGame();
        Task<int> GetBuddyId();
        Task Save(Game game);
        Task<Game> GetStoredGame();
        Task<bool> IsPublished();
        Task<string> ShowCoworker();
        Task<string> ShowScenarioText(int number);
        Task<List<string>> ShowChoices(int number);
        Task<string> ChoiceMade(int numberScenario, int numberAdvice);
        Task<string> ShowConclusion();
        Game CreateGameFromInput(
            int buddyId,
            string buddyName,
            string buddyIntroduction,
            IReadOnlyList<(string scenarioText, IReadOnlyList<(string advice, string feedback)> choices)> scenarios,
            string conclusion,
            bool publish = true);
        void PublishGame(Game existingGame);
        void UnpublishGame(Game existingGame);
    }
}