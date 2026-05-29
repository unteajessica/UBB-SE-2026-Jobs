namespace Tests_and_Interviews.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains constant values used throughout the test management system.
    /// </summary>
    public static class TestConstants
    {
        /// <summary>
        /// The duration of a test in minutes.
        /// </summary>
        public const int TestDurationInMinutes = 30;

        /// <summary>
        /// The maximum number of questions per test.
        /// </summary>
        public const int MaxQuestionsPerTest = 25;

        /// <summary>
        /// The maximum score per question.
        /// </summary>
        public const float MaxScorePerQuestion = 4f;

        /// <summary>
        /// The maximum total score for a test.
        /// </summary>
        public const float MaxTotalScore = 100f;

        /// <summary>
        /// The number of options per multiple choice or single choice question.
        /// </summary>
        public const int OptionsPerQuestion = 6;
    }
}
