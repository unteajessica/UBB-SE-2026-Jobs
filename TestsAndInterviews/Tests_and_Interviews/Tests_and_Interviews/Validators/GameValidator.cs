using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests_and_Interviews.Validators
{
    public class GameValidator : IGameValidator
    {
        public const int MaxStruggleOrAdviceLength = 250;

        private const int RequiredScenarioCount = 2;
        private const int EmptyCollectionCount = 0;
        private const int DisplayIndexOffset = 1;

        public bool ValidateMandatoryFields(
            IReadOnlyList<(string scenarioText, IReadOnlyList<(string adviceText, string feedbackText)> choices)> scenarios)
        {
            if (scenarios == null || scenarios.Count != RequiredScenarioCount)
            {
                throw new Exception("Both scenarios are required to activate the game.");
            }

            for (int scenarioIndex = 0; scenarioIndex < scenarios.Count; scenarioIndex++)
            {
                var (scenarioText, choices) = scenarios[scenarioIndex];
                if (string.IsNullOrWhiteSpace(scenarioText))
                {
                    throw new Exception($"Scenario {scenarioIndex + DisplayIndexOffset}: the struggle description is mandatory.");
                }

                if (choices == null || choices.Count == EmptyCollectionCount)
                {
                    throw new Exception($"Scenario {scenarioIndex + DisplayIndexOffset}: at least one advice option is required.");
                }

                for (int choiceIndex = 0; choiceIndex < choices.Count; choiceIndex++)
                {
                    var (adviceText, feedbackText) = choices[choiceIndex];
                    if (string.IsNullOrWhiteSpace(adviceText))
                    {
                        throw new Exception($"Scenario {scenarioIndex + DisplayIndexOffset}, option {choiceIndex + DisplayIndexOffset}: advice text is mandatory.");
                    }
                    if (string.IsNullOrWhiteSpace(feedbackText))
                    {
                        throw new Exception($"Scenario {scenarioIndex + DisplayIndexOffset}, option {choiceIndex + DisplayIndexOffset}: reaction text is mandatory.");
                    }
                }
            }

            return true;
        }

        public bool ValidateCharacterLimits(
            IReadOnlyList<(string scenarioText, IReadOnlyList<(string adviceText, string feedbackText)> choices)> scenarios)
        {
            if (scenarios == null)
            {
                return true;
            }

            for (int scenarioIndex = 0; scenarioIndex < scenarios.Count; scenarioIndex++)
            {
                var (scenarioText, choices) = scenarios[scenarioIndex];
                if (scenarioText != null && scenarioText.Length > MaxStruggleOrAdviceLength)
                {
                    throw new Exception(
                        $"Scenario {scenarioIndex + DisplayIndexOffset}: struggle text must be at most {MaxStruggleOrAdviceLength} characters for mobile readability.");
                }

                if (choices == null)
                {
                    continue;
                }

                for (int choiceIndex = 0; choiceIndex < choices.Count; choiceIndex++)
                {
                    var (adviceText, ignoredFeedbackText) = choices[choiceIndex];
                    if (adviceText != null && adviceText.Length > MaxStruggleOrAdviceLength)
                    {
                        throw new Exception(
                            $"Scenario {scenarioIndex + DisplayIndexOffset}, option {choiceIndex + DisplayIndexOffset}: advice must be at most {MaxStruggleOrAdviceLength} characters for mobile readability.");
                    }
                }
            }

            return true;
        }

        public bool ValidatePositiveConclusion(string conclusion)
        {
            if (string.IsNullOrWhiteSpace(conclusion))
            {
                throw new Exception("A conclusion is required so the game can end on a positive note.");
            }

            return true;
        }

        public bool ValidateForActivation(
            IReadOnlyList<(string scenarioText, IReadOnlyList<(string adviceText, string feedbackText)> choices)> scenarios,
            string conclusion)
        {
            ValidateMandatoryFields(scenarios);
            ValidateCharacterLimits(scenarios);
            ValidatePositiveConclusion(conclusion);
            return true;
        }
    }
}