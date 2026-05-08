using FluentAssertions;
using PussyCats.App.Configuration;
using PussyCats.App.ViewModels;

namespace PussyCats.Tests.Integration;

public class DispatchableObservableObjectTests
{
    [Fact]
    public void SetProperty_DispatcherNotAvailable_InvokesPropertyChangedSynchronously()
    {
        UIDispatcher.Queue = null;
        var viewModel = new TestDispatchableViewModel();
        var changedProperties = new List<string?>();
        viewModel.PropertyChanged += (_, eventArgs) => changedProperties.Add(eventArgs.PropertyName);

        viewModel.Name = "Ada";

        changedProperties.Should().ContainSingle(nameof(TestDispatchableViewModel.Name));
    }

    private sealed class TestDispatchableViewModel : DispatchableObservableObject
    {
        private string name = string.Empty;

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }
    }
}