using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;

namespace PussyCats.App.ViewModels;

public class ChatViewModel : DispatchableObservableObject
{
    private readonly IChatService chatService;
    private readonly SessionContext session;
    private Chat? selectedChat;
    private string messageText = string.Empty;
    private string searchQuery = string.Empty;
    private string statusMessage = string.Empty;
    private bool isBusy;

    public ChatViewModel(IChatService chatService, SessionContext session)
    {
        this.chatService = chatService;
        this.session = session;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        SendCommand = new AsyncRelayCommand(SendAsync, CanSend);
        BlockCommand = new AsyncRelayCommand(BlockAsync, () => SelectedChat is not null && !SelectedChat.IsBlocked);
        UnblockCommand = new AsyncRelayCommand(UnblockAsync, () => SelectedChat?.IsBlocked == true);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedChat is not null);
    }

    public ObservableCollection<Chat> Chats { get; } = new();
    public ObservableCollection<MessageDisplayViewModel> Messages { get; } = new();
    public ObservableCollection<ContactSearchResultViewModel> SearchResults { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand SearchCommand { get; }
    public IAsyncRelayCommand SendCommand { get; }
    public IAsyncRelayCommand BlockCommand { get; }
    public IAsyncRelayCommand UnblockCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }

    public Chat? SelectedChat
    {
        get => selectedChat;
        set
        {
            if (SetProperty(ref selectedChat, value))
            {
                RaiseActionStateChanged();
                _ = LoadSelectedChatAsync();
            }
        }
    }

    public string MessageText
    {
        get => messageText;
        set
        {
            if (SetProperty(ref messageText, value))
            {
                SendCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string SearchQuery
    {
        get => searchQuery;
        set => SetProperty(ref searchQuery, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set => SetProperty(ref isBusy, value);
    }

    public bool IsCompanyMode => session.Mode == AppMode.Company;
    public bool IsDeveloperMode => session.Mode == AppMode.Developer;

    public async Task LoadAsync()
    {
        await RunSafelyAsync(async () =>
        {
            Chats.Clear();
            Messages.Clear();
            SearchResults.Clear();

            var callerId = GetCallerId();
            var chats = session.Mode == AppMode.Company
                ? await chatService.GetChatsForCompanyAsync(callerId)
                : await chatService.GetChatsForUserAsync(callerId);

            foreach (var chat in chats)
            {
                Chats.Add(chat);
            }

            StatusMessage = Chats.Count == 0
                ? "No conversations yet. Search for a contact to start one."
                : string.Empty;
        });
    }

    public async Task SearchAsync()
    {
        await RunSafelyAsync(async () =>
        {
            SearchResults.Clear();
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                return;
            }

            if (session.Mode != AppMode.Company)
            {
                var companies = await chatService.SearchCompaniesAsync(SearchQuery);
                foreach (var company in companies)
                {
                    SearchResults.Add(ContactSearchResultViewModel.ForCompany(company));
                }
            }

            var users = await chatService.SearchUsersAsync(SearchQuery);
            foreach (var user in users.Where(user => user.UserId != session.UserId))
            {
                SearchResults.Add(ContactSearchResultViewModel.ForUser(user));
            }
        });
    }

    public async Task StartChatAsync(ContactSearchResultViewModel? result)
    {
        if (result is null)
        {
            return;
        }

        await RunSafelyAsync(async () =>
        {
            Chat? chat = null;
            if (result.Kind == ContactSearchResultKind.Company)
            {
                chat = await chatService.FindOrCreateUserCompanyChatAsync(session.UserId, result.Id);
            }
            else if (session.Mode == AppMode.Company && session.CompanyId is int companyId)
            {
                chat = await chatService.FindOrCreateUserCompanyChatAsync(result.Id, companyId);
            }
            else
            {
                chat = await chatService.FindOrCreateUserChatAsync(session.UserId, result.Id);
            }

            await LoadAsync();
            SelectedChat = Chats.FirstOrDefault(candidate => candidate.ChatId == chat?.ChatId);
            SearchQuery = string.Empty;
            SearchResults.Clear();
        });
    }

    private async Task LoadSelectedChatAsync()
    {
        Messages.Clear();
        if (SelectedChat is null)
        {
            return;
        }

        await RunSafelyAsync(async () =>
        {
            var callerId = GetCallerId();
            var messages = await chatService.GetMessagesAsync(SelectedChat.ChatId, callerId);
            await chatService.MarkMessagesAsReadAsync(SelectedChat.ChatId, callerId);
            foreach (var message in messages)
            {
                Messages.Add(new MessageDisplayViewModel(message, message.SenderId == callerId));
            }
        });
    }

    private async Task SendAsync()
    {
        if (SelectedChat is null || string.IsNullOrWhiteSpace(MessageText))
        {
            return;
        }

        var text = MessageText;
        MessageText = string.Empty;
        await RunSafelyAsync(async () =>
        {
            await chatService.SendMessageAsync(SelectedChat.ChatId, text, GetCallerId(), MessageType.Text);
            await LoadAsync();
            SelectedChat = Chats.FirstOrDefault(chat => chat.ChatId == selectedChat?.ChatId);
            await LoadSelectedChatAsync();
        });
    }

    private async Task BlockAsync()
    {
        if (SelectedChat is null)
        {
            return;
        }

        await RunSafelyAsync(async () =>
        {
            await chatService.BlockChatAsync(SelectedChat.ChatId, GetCallerId());
            await LoadAsync();
        });
    }

    private async Task UnblockAsync()
    {
        if (SelectedChat is null)
        {
            return;
        }

        await RunSafelyAsync(async () =>
        {
            await chatService.UnblockChatAsync(SelectedChat.ChatId, GetCallerId());
            await LoadAsync();
        });
    }

    private async Task DeleteAsync()
    {
        if (SelectedChat is null)
        {
            return;
        }

        await RunSafelyAsync(async () =>
        {
            await chatService.DeleteChatAsync(SelectedChat.ChatId, GetCallerId());
            SelectedChat = null;
            await LoadAsync();
        });
    }

    private bool CanSend()
    {
        return SelectedChat is not null && !SelectedChat.IsBlocked && !string.IsNullOrWhiteSpace(MessageText);
    }

    private int GetCallerId()
    {
        return session.Mode switch
        {
            AppMode.Company => session.CompanyId ?? throw new InvalidOperationException("No company session is active."),
            AppMode.Developer => session.DeveloperId ?? throw new InvalidOperationException("No developer session is active."),
            _ => session.UserId,
        };
    }

    private async Task RunSafelyAsync(Func<Task> action)
    {
        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            await action();
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
            RaiseActionStateChanged();
        }
    }

    private void RaiseActionStateChanged()
    {
        SendCommand.NotifyCanExecuteChanged();
        BlockCommand.NotifyCanExecuteChanged();
        UnblockCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
    }
}

public sealed class MessageDisplayViewModel
{
    public MessageDisplayViewModel(Message message, bool isMine)
    {
        IsMine = isMine;
        SenderLabel = isMine ? "Me" : "Them";
        Content = message.Content;
        Timestamp = message.Timestamp.ToLocalTime().ToString("HH:mm");
    }

    public bool IsMine { get; }
    public string SenderLabel { get; }
    public string Content { get; }
    public string Timestamp { get; }
}

public sealed class ContactSearchResultViewModel
{
    private ContactSearchResultViewModel(ContactSearchResultKind kind, int id, string displayName, string secondaryText)
    {
        Kind = kind;
        Id = id;
        DisplayName = displayName;
        SecondaryText = secondaryText;
    }

    public ContactSearchResultKind Kind { get; }
    public int Id { get; }
    public string DisplayName { get; }
    public string SecondaryText { get; }

    public static ContactSearchResultViewModel ForCompany(Company company)
    {
        return new ContactSearchResultViewModel(ContactSearchResultKind.Company, company.CompanyId, company.CompanyName, "Company");
    }

    public static ContactSearchResultViewModel ForUser(User user)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = $"User {user.UserId}";
        }

        return new ContactSearchResultViewModel(ContactSearchResultKind.User, user.UserId, fullName, user.Email);
    }
}

public enum ContactSearchResultKind
{
    User,
    Company,
}
