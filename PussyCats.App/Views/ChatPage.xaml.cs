using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;

namespace PussyCats_App.Views;

public sealed partial class ChatPage : Page
{
    public ChatPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ChatViewModel>();
        DataContext = ViewModel;
    }

    public ChatViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
        await ViewModel.LoadAsync();
    }

    private async void ConversationList_SelectionChanged(object sender, SelectionChangedEventArgs eventArguments)
    {
        await Task.CompletedTask;
    }

    private async void SearchResultsList_ItemClick(object sender, ItemClickEventArgs eventArguments)
    {
        await ViewModel.StartChatAsync(eventArguments.ClickedItem as ContactSearchResultViewModel);
    }

    private void MessageBox_KeyDown(object sender, KeyRoutedEventArgs eventArguments)
    {
        if (eventArguments.Key == Windows.System.VirtualKey.Enter && ViewModel.SendCommand.CanExecute(null))
        {
            ViewModel.SendCommand.Execute(null);
            eventArguments.Handled = true;
        }
    }
}
