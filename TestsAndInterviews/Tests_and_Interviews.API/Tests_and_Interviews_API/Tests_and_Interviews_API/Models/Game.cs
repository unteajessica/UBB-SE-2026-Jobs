namespace Tests_and_Interviews_API.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Game class represents a game scenario where a user is presented with multiple scenarios and advice choices.
    /// It contains properties for the buddy associated with the game, a list of scenarios, a conclusion, and a flag indicating whether the game is published or not.
    /// The Game class provides methods to retrieve specific scenarios, add new scenarios, and manage the publication status of the game.
    /// </summary>
    public class Game
    {
        private const int DefaultBuddyId = 0;

        /// <summary>
        /// Gets the conclusion of the game. This property represents the final outcome or summary of the game, providing closure and context for the scenarios and advice choices presented throughout the game.
        /// </summary>
        private readonly List<Scenario> scenarios;

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class with default values for the buddy, scenarios, conclusion, and publication status.
        /// </summary>
        public Game()
        {
            this.Buddy = new Buddy(DefaultBuddyId, string.Empty, string.Empty);
            this.scenarios = new List<Scenario>();
            this.Conclusion = string.Empty;
            this.IsPublished = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class with the specified buddy, list of scenarios, conclusion, and publication status.
        /// </summary>
        /// <param name="buddy">The buddy associated with the game.</param>
        /// <param name="scenarioList">The list of scenarios included in the game.</param>
        /// <param name="conclusion">The conclusion of the game.</param>
        /// <param name="isPublished">A value indicating whether the game is published or not.</param>
        /// <exception cref="ArgumentNullException">Thrown when the buddy or scenarioList is null.</exception>
        public Game(Buddy buddy, IEnumerable<Scenario> scenarioList, string conclusion, bool isPublished = false)
        {
            this.Buddy = buddy ?? throw new ArgumentNullException(nameof(buddy));
            this.scenarios = scenarioList?.ToList() ?? throw new ArgumentNullException(nameof(scenarioList));
            this.Conclusion = conclusion ?? string.Empty;
            this.IsPublished = isPublished;
        }

        /// <summary>
        /// Gets the buddy associated with the game. This property represents the character or persona that the user interacts with during the game, providing context
        /// and guidance throughout the scenarios presented in the game.
        /// </summary>
        public Buddy Buddy { get; private set; }

        /// <summary>
        /// Gets the read-only list of scenarios included in the game. Each scenario represents a situation or context in which the user is presented with multiple advice choices,
        /// allowing for an interactive and engaging gaming experience.
        /// </summary>
        public IReadOnlyList<Scenario> Scenarios => this.scenarios;

        /// <summary>
        /// Gets the conclusion of the game. This property represents the final outcome or summary of the game, providing closure and context for the scenarios and advice choices presented throughout the game.
        /// </summary>
        public string Conclusion { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the game is published or not. This property represents the publication status of the game, allowing for control over when the game is made available to users and when it is still in development or testing stages.
        /// </summary>
        public bool IsPublished { get; private set; }

        /// <summary>
        /// Retrieves a specific scenario from the game based on the provided index. This method allows for accessing individual scenarios within the game, enabling users to navigate through the different situations and advice choices presented in the game.
        /// </summary>
        /// <param name="index">The index of the scenario to retrieve.</param>
        /// <returns>The scenario at the specified index.</returns>
        public Scenario GetScenario(int index)
        {
            return this.scenarios[index];
        }

        /// <summary>
        /// Adds a new scenario to the game. This method allows for expanding the game by introducing new situations and advice choices, enhancing the overall gaming experience and providing users with more opportunities for interaction and engagement.
        /// </summary>
        /// <param name="scenario">The scenario to add to the game.</param>
        public void AddScenario(Scenario scenario)
        {
            this.scenarios.Add(scenario);
        }

        /// <summary>
        /// Publishes the game by setting the IsPublished property to true. This method allows for making the game available to users, indicating that it is ready for public access and interaction. Once published, the game can be accessed and played by users, providing them with the scenarios and advice choices included in the game.
        /// </summary>
        public void Publish()
        {
            this.IsPublished = true;
        }

        /// <summary>
        /// Unpublishes the game by setting the IsPublished property to false. This method allows for retracting the game from public access, indicating that it is no longer available for users to play. Unpublishing a game can be useful for making updates, fixing issues, or temporarily removing the game from circulation while it is being modified or improved.
        /// </summary>
        public void Unpublish()
        {
            this.IsPublished = false;
        }
    }
}