namespace Tests_and_Interviews.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// AdviceChoice class represents a choice of advice that a user can select in a game scenario, along with feedback associated with that choice.
    /// It contains properties for the advice text and feedback, and provides a method to determine if the advice choice is selected based on the feedback provided.
    /// </summary>
    public class AdviceChoice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdviceChoice"/> class with the specified advice text and feedback. 
        /// This constructor allows for creating an advice choice with specific attributes, providing a clear and concise representation of the advice option and its associated feedback for users to make informed decisions in game scenarios.
        /// </summary>
        /// <param name="advice">The advice text for this choice.</param>
        /// <param name="feedback">The feedback associated with this advice choice.</param>
        public AdviceChoice(string advice, string feedback)
        {
            this.Advice = advice;
            this.Feedback = feedback;
        }

        /// <summary>
        /// Gets the advice text for this choice. This property represents the specific advice or recommendation that is presented to the user as an option in a game scenario, allowing the user to make a selection based on the advice provided.
        /// </summary>
        public string Advice { get; private set; }

        /// <summary>
        /// Gets the feedback associated with this advice choice. This property represents the feedback or response that is provided to the user after selecting the advice, allowing the user to understand the consequences or outcomes of their choice.
        /// </summary>
        public string Feedback { get; private set; }

        /// <summary>
        /// Determines if the advice choice is selected based on the feedback provided. This method evaluates the feedback associated with the advice choice to determine if it indicates that the choice has been selected or not, allowing for dynamic interactions and responses in game scenarios based on user selections.
        /// </summary>
        /// <returns>A string containing the feedback.</returns>
        public string IsChosen()
        {
            return this.Feedback;
        }
    }
}
