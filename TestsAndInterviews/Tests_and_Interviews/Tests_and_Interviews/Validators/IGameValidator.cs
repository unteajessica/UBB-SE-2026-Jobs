using System.Collections.Generic;

namespace Tests_and_Interviews.Validators
{
    public interface IGameValidator
    {
        bool ValidateMandatoryFields(
            IReadOnlyList<(string scenarioText, IReadOnlyList<(string adviceText, string feedbackText)> choices)> scenarios);
        bool ValidateCharacterLimits(
            IReadOnlyList<(string scenarioText, IReadOnlyList<(string adviceText, string feedbackText)> choices)> scenarios);
        bool ValidatePositiveConclusion(string conclusion);
        bool ValidateForActivation(
            IReadOnlyList<(string scenarioText, IReadOnlyList<(string adviceText, string feedbackText)> choices)> scenarios,
            string conclusion);
    }
}