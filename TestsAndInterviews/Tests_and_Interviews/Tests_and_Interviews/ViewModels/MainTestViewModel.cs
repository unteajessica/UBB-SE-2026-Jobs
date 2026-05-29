namespace Tests_and_Interviews.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.UI.Xaml;
    using Tests_and_Interviews.Services.Interfaces;

    /// <summary>
    /// MainTestViewModel represents the view model for the main test listing page in the application.
    /// It manages the state and data for displaying a list of tests, including loading state, selected test, and
    /// visibility of UI elements based on the presence of tests. The view model interacts with a TestRepository
    /// to load tests from a data source and provides properties and methods to support data binding in the UI.
    /// </summary>
    public partial class MainTestViewModel : INotifyPropertyChanged
    {
        private readonly ITestService testService;
        private bool isLoading = false;
        private TestCardViewModel? selectedTest;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainTestViewModel"/> class.
        /// MainTestViewModel constructor initializes a new instance of the MainTestViewModel class and sets up the necessary dependencies, such as the TestRepository.
        /// </summary>
        /// <param name="testService">An instance of the ITestRepository interface used to interact with the data source for loading tests.</param>
        public MainTestViewModel(ITestService testService)
        {
            this.testService = testService;
        }

        /// <summary>
        /// PropertyChanged event is raised whenever a property value changes, allowing the UI to update accordingly.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the collection of TestCardViewModel instances representing the tests to be displayed in the UI.
        /// </summary>
        public ObservableCollection<TestCardViewModel> Tests { get; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether the view model is currently loading test data.
        /// This property is used to control the display of loading indicators in the UI and to determine the visibility of certain UI elements
        /// based on whether tests are being loaded or have been loaded.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set
            {
                this.isLoading = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.NoTestsVisible));
            }
        }

        /// <summary>
        /// Gets or sets the currently selected test card in the UI. When a new test card is selected, the previous selection is deselected,
        /// and the new selection is marked as selected. This property is used to manage the selection state of test cards in the UI and to trigger
        /// updates to the visual appearance of the selected card.
        /// </summary>
        public TestCardViewModel? SelectedTest
        {
            get => this.selectedTest;
            set
            {
                if (this.selectedTest != null)
                {
                    this.selectedTest.IsSelected = false;
                }

                this.selectedTest = value;
                if (this.selectedTest != null)
                {
                    this.selectedTest.IsSelected = true;
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the visibility of a UI element indicating that no tests are available.
        /// </summary>
        public Visibility NoTestsVisible =>
            (!this.IsLoading && this.Tests.Count == 0) ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Async method to load tests from the TestRepository. This method sets the IsLoading property to true while loading, clears the existing tests collection,
        /// </summary>
        /// <returns>A Task representing the asynchronous operation of loading tests.</returns>
        public async Task LoadTestsAsync()
        {
            this.IsLoading = true;
            this.Tests.Clear();

            var categories = new List<string> { "Programming", "Database", "Computer Science" };

            foreach (var category in categories)
            {
                var tests = await this.testService.FindTestsByCategoryAsync(category);

                foreach (var test in tests)
                {
                    string typeLabel = "MIXED";

                    if (test.Questions != null && test.Questions.Count > 0)
                    {
                        typeLabel = test.Questions[0].QuestionTypeString.Replace("_", "/");
                    }

                    this.Tests.Add(new TestCardViewModel
                    {
                        TestId = test.Id,
                        Title = test.Title,
                        Category = test.Category,
                        QuestionTypeLabel = typeLabel,
                    });
                }
            }

            this.IsLoading = false;
            this.OnPropertyChanged(nameof(this.NoTestsVisible));
        }

        /// <summary>
        /// OnPropertyChanged method is a helper method that raises the PropertyChanged event for a given property name, enabling the UI
        /// to react to changes in the view model's properties.
        /// </summary>
        /// <param name="name">The name of the property that changed. This parameter is optional and defaults to the caller member name.</param>
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}