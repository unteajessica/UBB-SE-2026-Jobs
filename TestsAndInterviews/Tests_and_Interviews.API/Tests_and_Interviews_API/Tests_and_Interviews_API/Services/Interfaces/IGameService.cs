using System.Collections.Generic;
using Tests_and_Interviews_API.Models;

namespace Tests_and_Interviews_API.Services.Interfaces
{
    public interface IGameService
    {
        Game LoadedGame();
        int GetBuddyId();
        void Save(Game game);
        Game GetStoredGame();
        bool IsPublished();
        string ShowCoworker();
        string ShowScenarioText(int number);
        List<string> ShowChoices(int number);
        string ChoiceMade(int numberScenario, int numberAdvice);
        string ShowConclusion();
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