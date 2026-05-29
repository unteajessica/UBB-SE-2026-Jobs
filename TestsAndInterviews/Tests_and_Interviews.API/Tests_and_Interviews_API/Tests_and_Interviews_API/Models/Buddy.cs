namespace Tests_and_Interviews_API.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Buddy class represents a character or persona that the user interacts with during a game scenario, providing context and guidance throughout the scenarios presented in the game.
    /// </summary>
    public class Buddy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Buddy"/> class with the specified id, name, and introduction. This constructor allows for creating a buddy with specific attributes, providing a personalized and engaging character for users to interact with during game scenarios.
        /// </summary>
        /// <param name="id">The unique identifier of the buddy.</param>
        /// <param name="name">The name of the buddy.</param>
        /// <param name="introduction">The introduction of the buddy.</param>
        public Buddy(int id, string name, string introduction)
        {
            this.Id = id;
            this.Name = name;
            this.Introduction = introduction;
        }

        /// <summary>
        /// Gets the unique identifier of the buddy. This property represents a distinct value that can be used to identify and differentiate between different buddies within the game, allowing for personalized interactions and experiences based on the specific buddy associated with each game scenario.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the name of the buddy. This property represents the name of the character or persona that the user interacts with during a game scenario, providing context and guidance throughout the scenarios presented in the game.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the introduction of the buddy. This property represents a brief description or background information about the buddy, providing additional context and insight into the character or persona that the user interacts with during a game scenario, enhancing the overall gaming experience and immersion for the user.
        /// </summary>
        public string Introduction { get; private set; }
    }
}
