using System.Collections.Specialized;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using PussyCats.App.ViewModels;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.DTOs;
using Windows.Storage.Pickers;

namespace PussyCats_App.Views;

public sealed partial class ChatPage : Page
{
    private const double RefreshIntervalSeconds = 3;
    private readonly DispatcherTimer refreshTimer;
    private bool isScrollToLatestQueued;

    public ChatPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ChatViewModel>();
        DataContext = ViewModel;

        refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(RefreshIntervalSeconds),
        };
        refreshTimer.Tick += RefreshTimer_Tick;
    }

    public ChatViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs eventArguments)
    {
        base.OnNavigatedTo(eventArguments);
        await ViewModel.LoadAsync();
        ViewModel.ProfileNavigationRequested += OnProfileNavigationRequested;
        ViewModel.CompanyNavigationRequested += OnCompanyNavigationRequested;
        ViewModel.JobNavigationRequested += OnJobNavigationRequested;
        ViewModel.Messages.CollectionChanged -= Messages_CollectionChanged;
        ViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
        refreshTimer.Start();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs eventArguments)
    {
        refreshTimer.Stop();
        ViewModel.ProfileNavigationRequested -= OnProfileNavigationRequested;
        ViewModel.CompanyNavigationRequested -= OnCompanyNavigationRequested;
        ViewModel.JobNavigationRequested -= OnJobNavigationRequested;
        ViewModel.Messages.CollectionChanged -= Messages_CollectionChanged;
        base.OnNavigatedFrom(eventArguments);
    }

    private void ConversationList_SelectionChanged(object sender, SelectionChangedEventArgs eventArguments)
    {
        if (eventArguments.AddedItems.Count > 0 && eventArguments.AddedItems[0] is Chat chat)
        {
            ViewModel.SelectChat(chat);
            QueueScrollToLatestMessage();
        }
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs eventArguments)
    {
        if (eventArguments.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            _ = ViewModel.SearchAsync();
        }
    }

    private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs eventArguments)
    {
    }

    private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs eventArguments)
    {
        if (eventArguments.ChosenSuggestion is ContactSearchResultViewModel result)
        {
            await ViewModel.StartChatAsync(result);
            return;
        }

        await ViewModel.SearchAsync();
    }

    private void MessageInput_KeyDown(object sender, KeyRoutedEventArgs eventArguments)
    {
        if (eventArguments.Key == Windows.System.VirtualKey.Enter)
        {
            if (ViewModel.SendCommand.CanExecute(null))
            {
                ViewModel.SendCommand.Execute(null);
            }
            eventArguments.Handled = true;
        }
    }

    private void HandleSendButtonClick(object sender, RoutedEventArgs eventArguments)
    {
        if (ViewModel.SendCommand.CanExecute(null))
        {
            ViewModel.SendCommand.Execute(null);
        }
    }

    private async void HandleAttachmentButtonClick(object sender, RoutedEventArgs eventArguments)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".pdf");
        picker.FileTypeFilter.Add(".doc");
        picker.FileTypeFilter.Add(".docx");

        var handle = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, handle);

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        ViewModel.HandleAttachmentSelected(file.Path);
    }

    private void HandleGoToProfileClick(object sender, RoutedEventArgs eventArguments)
    {
        ViewModel.GoToProfile();
    }

    private void HandleGoToCompanyProfileClick(object sender, RoutedEventArgs eventArguments)
    {
        ViewModel.GoToCompanyProfile();
    }

    private void HandleGoToJobPostClick(object sender, RoutedEventArgs eventArguments)
    {
        ViewModel.GoToJobPost();
    }

    private void HandleBlockButtonClick(object sender, RoutedEventArgs eventArguments)
    {
        if (ViewModel.BlockCommand.CanExecute(null))
        {
            ViewModel.BlockCommand.Execute(null);
        }
    }

    private void HandleUnblockButtonClick(object sender, RoutedEventArgs eventArguments)
    {
        if (ViewModel.UnblockCommand.CanExecute(null))
        {
            ViewModel.UnblockCommand.Execute(null);
        }
    }

    private async void HandleDeleteChatButtonClick(object sender, RoutedEventArgs eventArguments)
    {
        var dialog = new ContentDialog
        {
            Title = "Delete conversation",
            Content = "Delete this conversation? This action cannot be undone.",
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            if (ViewModel.DeleteCommand.CanExecute(null))
            {
                ViewModel.DeleteCommand.Execute(null);
            }
        }
    }

    private void UsersTab_Click(object sender, RoutedEventArgs eventArguments)
    {
        ViewModel.SwitchTab("Users");
    }

    private void CompanyTab_Click(object sender, RoutedEventArgs eventArguments)
    {
        ViewModel.SwitchTab("Company");
    }

    private void RefreshTimer_Tick(object? sender, object eventArguments)
    {
        _ = ViewModel.RefreshInboxAndSelectedChatAsync();
    }

    private void OnProfileNavigationRequested(int userId)
    {
        Frame.Navigate(typeof(Views.Candidate.PublicProfilePage), userId);
    }

    private void OnCompanyNavigationRequested(int companyId)
    {
        Frame.Navigate(typeof(Views.Candidate.CompanyProfilePage), companyId);
    }

    private void OnJobNavigationRequested(int jobId)
    {
        var job = ViewModel.LinkedJob;
        if (job is null)
        {
            return;
        }

        var model = new ApplicationCardModel
        {
            JobId = job.JobId,
            CompanyName = job.Company?.CompanyName ?? string.Empty,
            JobDescription = job.JobDescription,
            AppliedDate = DateTime.Now,
            Status = MatchStatus.Applied,
        };
        Frame.Navigate(typeof(Views.Candidate.UserStatusJobDetailPage), model);
    }

    private async void MessageList_ItemClick(object sender, ItemClickEventArgs eventArguments)
    {
        if (eventArguments.ClickedItem is not MessageDisplayViewModel message)
        {
            return;
        }

        await DownloadAttachmentAsync(message.Message);
    }

    private async void AttachmentMessage_Click(object sender, RoutedEventArgs eventArguments)
    {
        if (sender is not Button { Tag: Message message })
        {
            return;
        }

        await DownloadAttachmentAsync(message);
    }

    private async Task DownloadAttachmentAsync(Message message)
    {
        if (message.Type != MessageType.File && message.Type != MessageType.Image)
        {
            return;
        }

        var displayName = !string.IsNullOrWhiteSpace(message.OriginalFileName)
            ? message.OriginalFileName
            : Path.GetFileName(message.Content);

        var extension = Path.GetExtension(displayName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".bin";
        }

        var picker = new FileSavePicker
        {
            SuggestedFileName = displayName,
            DefaultFileExtension = extension,
        };
        picker.FileTypeChoices.Add("File", new List<string> { extension });

        var handle = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, handle);

        var file = await picker.PickSaveFileAsync();
        if (file is null)
        {
            return;
        }

        await ViewModel.DownloadAttachmentAsync(message, file.Path);
    }

    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs eventArguments)
    {
        if (eventArguments.Action == NotifyCollectionChangedAction.Add ||
            eventArguments.Action == NotifyCollectionChangedAction.Reset ||
            eventArguments.Action == NotifyCollectionChangedAction.Replace)
        {
            QueueScrollToLatestMessage();
        }
    }

    private void QueueScrollToLatestMessage()
    {
        if (isScrollToLatestQueued)
        {
            return;
        }

        isScrollToLatestQueued = true;
        DispatcherQueue.TryEnqueue(HandleQueuedScrollToLatestMessage);
    }

    private void HandleQueuedScrollToLatestMessage()
    {
        isScrollToLatestQueued = false;
        ScrollToLatestMessage();
    }

    private void ScrollToLatestMessage()
    {
        if (MessageList.Items.Count == 0)
        {
            return;
        }

        var lastMessage = MessageList.Items[^1];
        MessageList.ScrollIntoView(lastMessage);
    }
}
