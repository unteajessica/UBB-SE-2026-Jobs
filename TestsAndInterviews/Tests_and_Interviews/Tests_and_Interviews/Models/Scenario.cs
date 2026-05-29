namespace Tests_and_Interviews.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Scenario class represents a situation or context in which a user is presented with multiple advice choices.
    /// It contains a description of the scenario and a list of advice choices that the user can select from.
    /// Each advice choice includes the advice text and feedback for that choice.
    /// The Scenario class provides methods to retrieve the advice texts, feedback, and to select a specific choice based on an index.
    /// </summary>
    public class Scenario
    {
        /// <summary>
        /// Gets the list of advice choices available in this scenario. Each advice choice includes the advice text and feedback for that choice.
        /// </summary>
        private List<AdviceChoice> choices;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scenario"/> class with the specified description.
        /// The constructor also initializes the list of advice choices to an empty list, allowing for advice choices to be added later using the AddChoice method.
        /// </summary>
        /// <param name="description">The description of the scenario, providing context for the advice choices presented to the user.</param>
        public Scenario(string description)
        {
            this.Description = description;
            this.choices = new List<AdviceChoice>();
        }

        /// <summary>
        /// Gets the description of the scenario, providing context for the advice choices presented to the user.
        /// This property is read-only and is set through the constructor when creating a new instance of the Scenario class.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the read-only list of advice choices available in this scenario. Each advice choice includes the advice text and feedback for that choice.
        /// </summary>
        public IReadOnlyList<AdviceChoice> AdviceChoices => this.choices;

        /// <summary>
        /// Adds a new advice choice to the scenario. This method allows for dynamically adding advice choices to the scenario after it has been created.
        /// </summary>
        /// <param name="choice">The choice to be added to the scenario.</param>
        public void AddChoice(AdviceChoice choice)
        {
            this.choices.Add(choice);
        }

        /// <summary>
        /// Gets the list of advice texts from the advice choices available in this scenario. This method iterates through the list of advice choices and extracts the advice text from each choice, returning a list of strings that represent the advice options presented to the user.
        /// </summary>
        /// <returns>A list of strings representing the advice texts from the advice choices.</returns>
        public List<string> GetAdviceTexts()
        {
            List<string> adviceTexts = new List<string>();

            for (int index = 0; index < this.choices.Count; index++)
            {
                adviceTexts.Add(this.choices[index].Advice);
            }

            return adviceTexts;
        }

        /// <summary>
        /// Gets the list of feedback texts from the advice choices available in this scenario. This method iterates through the list of advice choices and extracts the feedback text from each choice, returning a list of strings that represent the feedback for each advice option presented to the user.
        /// </summary>
        /// <returns>A list of strings representing the feedback texts from the advice choices.</returns>
        public List<string> GetAdviceReactions()
        {
            List<string> adviceReactions = new List<string>();

            for (int index = 0; index < this.choices.Count; index++)
            {
                adviceReactions.Add(this.choices[index].Feedback);
            }

            return adviceReactions;
        }

        /// <summary>
        /// Gets the advice text for the selected choice based on the provided index. This method checks if the index is within the valid range of advice choices and returns the advice text for the corresponding choice. If the index is out of range, it throws an ArgumentOutOfRangeException to indicate that an invalid choice index was provided.
        /// </summary>
        /// <param name="index">The index of the choice to select.</param>
        /// <returns>The advice text for the selected choice.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided index is out of range.</exception>
        public string SelectChoice(int index)
        {
            if (index < 0 || index >= this.choices.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid choice index");
            }

            return this.choices[index].IsChosen();
        }
    }
}