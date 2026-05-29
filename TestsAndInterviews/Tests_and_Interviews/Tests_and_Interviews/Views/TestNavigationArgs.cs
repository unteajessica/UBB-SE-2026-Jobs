namespace Tests_and_Interviews.Views
{
    /// <summary>
    /// TestNavigationArgs class represents the arguments that are passed when navigating to the test view.
    /// It contains the TestId and UserId properties, which are used to identify the specific test and user
    /// for which the test view is being displayed.
    /// </summary>
    public class TestNavigationArgs
    {
        /// <summary>
        /// Gets or sets the unique identifier of the test.
        /// This property is used to identify which test should be displayed in the test view.
        /// </summary>
        public int TestId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public int UserId { get; set; }
    }
}