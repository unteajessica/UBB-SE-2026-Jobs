using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests_and_Interviews.Validators;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace TestsAndInterviews.Tests.Validators
{
    [TestClass]
    public class GameValidatorTests
    {
        private const char CharFiller = 'a';
        private const int ExceededLength = 251;
        private const int ValidScenarioIndex = 0;

        private const string StringEmpty = "";

        private const string ScenarioOneText = "First scenario";
        private const string AdviceOne = "Advice 1";
        private const string ReactionOne = "Reaction 1";
        private const string AdviceTwo = "Advice 2";
        private const string ReactionTwo = "Reaction 2";

        private const string ScenarioTwoText = "Second scenario";
        private const string AdviceThree = "Advice 3";
        private const string ReactionThree = "Reaction 3";

        private const string TestScenarioOne = "Scenario 1";
        private const string TestScenarioTwo = "Scenario 2";
        private const string TestAdvice = "Advice";
        private const string TestReaction = "Reaction";

        private const string InvalidSingleScenario = "Only one scenario";

        private const string ValidConclusion = "Well done!";
        private const string EndingConclusion = "Good ending";

        private GameValidator validator = null!;

        [TestInitialize]
        public void Setup()
        {
            validator = new GameValidator();
        }

        private List<(string scenarioText, IReadOnlyList<(string advice, string feedback)> choices)> GetValidScenarios()
        {
            return new List<(string, IReadOnlyList<(string, string)>)>
            {
                (ScenarioOneText, new List<(string, string)>
                    {
                        (AdviceOne, ReactionOne),
                        (AdviceTwo, ReactionTwo)
                    }),

                (ScenarioTwoText, new List<(string, string)>
                    {
                        (AdviceThree, ReactionThree)
                    })
            };
        }

        [TestMethod]
        public void MandatoryFieldsValidator_ValidScenarios_ReturnsTrue()
        {
            var scenarios = GetValidScenarios();
            var result = validator.ValidateMandatoryFields(scenarios);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_NullScenarios_ThrowsException()
        {
            List<(string, IReadOnlyList<(string, string)>)> scenarios = null!;
            Action action = () => validator.ValidateMandatoryFields(scenarios);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_ScenarioTextMissing_ThrowsException()
        {
            var scenarios = GetValidScenarios();
            scenarios[ValidScenarioIndex] = (StringEmpty, scenarios[ValidScenarioIndex].choices);
            Action action = () => validator.ValidateMandatoryFields(scenarios);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_NoChoices_ThrowsException()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                (TestScenarioOne, new List<(string, string)>()),
                (TestScenarioTwo, new List<(string, string)> { (TestAdvice, TestReaction) })
            };

            Action action = () => validator.ValidateMandatoryFields(scenarios);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_AdviceMissing_ThrowsException()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                (TestScenarioOne, new List<(string, string)>
                {
                    (StringEmpty, TestReaction)
                }),

                (TestScenarioTwo, new List<(string, string)>
                {
                    (TestAdvice, TestReaction)
                })
            };

            Action action = () => validator.ValidateMandatoryFields(scenarios);

            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_FeedbackMissing_ThrowsException()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
            (TestScenarioOne, new List<(string, string)>
                {
                    (TestAdvice, StringEmpty)
                }),

                (TestScenarioTwo, new List<(string, string)>
                {
                    (TestAdvice, TestReaction)
                })
            };

            Action action = () => validator.ValidateMandatoryFields(scenarios);

            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_NotTwoScenarios_ThrowsException()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                (InvalidSingleScenario, new List<(string, string)>
                {
                    (TestAdvice, TestReaction)
                })
            };

            Action action = () => validator.ValidateMandatoryFields(scenarios);

            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void CharacterLimitsValidator_ValidScenario_ReturnsTrue()
        {
            var scenarios = GetValidScenarios();
            var result = validator.ValidateCharacterLimits(scenarios);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CharacterLimitsValidator_ScenarioTooLong_ThrowsException()
        {
            var scenarios = GetValidScenarios();
            scenarios[ValidScenarioIndex] = (new string(CharFiller, ExceededLength), scenarios[ValidScenarioIndex].choices);
            Action action = () => validator.ValidateCharacterLimits(scenarios);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void CharacterLimitsValidator_AdviceTooLong_ThrowsException()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                (TestScenarioOne, new List<(string, string)>
                {
                    (new string(CharFiller, ExceededLength), TestReaction)
                }),

                (TestScenarioTwo, new List<(string, string)>
                {
                    (TestAdvice, TestReaction)
                })
            };

            Action action = () => validator.ValidateCharacterLimits(scenarios);

            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void CharacterLimitsValidator_NullScenarios_ReturnsTrue()
        {
            List<(string, IReadOnlyList<(string, string)>)> scenarios = null!;
            var result = validator.ValidateCharacterLimits(scenarios);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CharacterLimitsValidator_NullChoices_ReturnsTrue()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                (TestScenarioOne, null!),
                (TestScenarioTwo, new List<(string, string)> { (TestAdvice, TestReaction) })
            };

            var result = validator.ValidateCharacterLimits(scenarios);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CharacterLimitsValidator_ScenarioAtLimit_ReturnsTrue()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                (new string(CharFiller, GameValidator.MaxStruggleOrAdviceLength),
                 new List<(string, string)>
                 {
                    (TestAdvice, TestReaction)
                 }),

                (TestScenarioTwo, new List<(string, string)>
                {
                    (TestAdvice, TestReaction)
                })
            };

            var result = validator.ValidateCharacterLimits(scenarios);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ConclusionPositiveValidator_ValidConclusion_ReturnsTrue()
        {
            var result = validator.ValidatePositiveConclusion(ValidConclusion);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ConclusionPositiveValidator_EmptyConclusion_ThrowsException()
        {
            Action action = () => validator.ValidatePositiveConclusion(StringEmpty);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void ValidateForActivation_ValidData_ReturnsTrue()
        {
            var scenarios = GetValidScenarios();
            var result = validator.ValidateForActivation(scenarios, EndingConclusion);
            Assert.IsTrue(result);
        }
    }
}