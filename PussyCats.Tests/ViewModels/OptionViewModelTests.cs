using Tests_and_Interviews.ViewModels;

namespace PussyCats.Tests.ViewModels
{
    public class OptionViewModelTests
    {
        [Fact]
        public void IsSelected_WhenChanged_RaisesPropertyChanged()
        {
            var option = new OptionViewModel();

            string? raisedProperty = null;

            option.PropertyChanged += (sender, eventArgs) =>
                raisedProperty = eventArgs.PropertyName;

            option.IsSelected = true;

            Assert.Equal("IsSelected", raisedProperty);
        }

        [Fact]
        public void IsSelected_WhenChanged_InvokesOnSelectionChanged()
        {
            var option = new OptionViewModel();

            var invoked = false;

            option.OnSelectionChanged = () => invoked = true;

            option.IsSelected = true;

            Assert.True(invoked);
        }

        [Fact]
        public void IsSelected_WhenOnSelectionChangedIsNull_DoesNotThrow()
        {
            var option = new OptionViewModel
            {
                OnSelectionChanged = null,
            };

            var exception = Record.Exception(() => option.IsSelected = true);

            Assert.Null(exception);
        }
    }
}