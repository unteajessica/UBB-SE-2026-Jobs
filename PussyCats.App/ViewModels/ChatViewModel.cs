using System.Collections.ObjectModel;
using System.IO;
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
    private readonly IJobService jobService;
    private readonly SessionContext session;
    private Chat? selectedChat;
    private string messageText = string.Empty;
    private string searchQuery = string.Empty;
    private string statusMessage = string.Empty;
    private string errorMessage = string.Empty;
    private bool isBusy;
    private string activeTab = "Users";
    private bool isUsersTabActive = true;
    private bool isCompaniesTabActive;
    private bool isSyncingTabState;
    private Job? linkedJob;
    private bool showBlock;
    private bool showUnblock;
    private bool showGoToProfile;
    private bool showGoToCompanyProfile;
    private bool showGoToJobPost;

    public ChatViewModel(IChatService chatService, IJobService jobService, SessionContext session)
    {
        this.chatService = chatService;
        this.jobService = jobService;
        this.session = session;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        SendCommand = new AsyncRelayCommand(SendAsync, CanSend);
        BlockCommand = new AsyncRelayCommand(BlockAsync, () => SelectedChat is not null && !SelectedChat.IsBlocked);
        UnblockCommand = new AsyncRelayCommand(UnblockAsync, () => SelectedChat?.IsBlocked == true);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedChat is not null);
    }

    public ObservableCollection<Chat> Chats { get; } = new();
    public ObservableCollection<Chat> FilteredChats { get; } = new();
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

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set => SetProperty(ref isBusy, value);
    }

    public string ActiveTab
    {
        get => activeTab;
        set
        {
            if (SetProperty(ref activeTab, value))
            {
                SyncTabTogglesFromActiveTab();
            }
        }
    }

    public bool IsUsersTabActive
    {
        get => isUsersTabActive;
        set
        {
            if (isSyncingTabState)
            {
                SetProperty(ref isUsersTabActive, value);
                return;
            }

            if (!SetProperty(ref isUsersTabActive, value))
            {
                return;
            }

            if (value && IsCandidateMode && ActiveTab != "Users")
            {
                SwitchTab("Users");
            }
            else if (!value && !isCompaniesTabActive)
            {
                isSyncingTabState = true;
                SetProperty(ref isUsersTabActive, true);
                isSyncingTabState = false;
            }
        }
    }

    public bool IsCompaniesTabActive
    {
        get => isCompaniesTabActive;
        set
        {
            if (isSyncingTabState)
            {
                SetProperty(ref isCompaniesTabActive, value);
                return;
            }

            if (!SetProperty(ref isCompaniesTabActive, value))
            {
                return;
            }

            if (value && IsCandidateMode && ActiveTab != "Company")
            {
                SwitchTab("Company");
            }
            else if (!value && !isUsersTabActive)
            {
                isSyncingTabState = true;
                SetProperty(ref isCompaniesTabActive, true);
                isSyncingTabState = false;
            }
        }
    }

    public Job? LinkedJob
    {
        get => linkedJob;
        private set => SetProperty(ref linkedJob, value);
    }

    public bool ShowBlock
    {
        get => showBlock;
        private set => SetProperty(ref showBlock, value);
    }

    public bool ShowUnblock
    {
        get => showUnblock;
        private set => SetProperty(ref showUnblock, value);
    }

    public bool ShowGoToProfile
    {
        get => showGoToProfile;
        private set => SetProperty(ref showGoToProfile, value);
    }

    public bool ShowGoToCompanyProfile
    {
        get => showGoToCompanyProfile;
        private set => SetProperty(ref showGoToCompanyProfile, value);
    }

    public bool ShowGoToJobPost
    {
        get => showGoToJobPost;
        private set => SetProperty(ref showGoToJobPost, value);
    }

    public bool IsCompanyMode => session.Mode == AppMode.Company;
    public bool IsDeveloperMode => session.Mode == AppMode.Developer;
    public bool IsCandidateMode => session.Mode == AppMode.Candidate;
    public bool IsUserMode => session.Mode == AppMode.Candidate;

    public ObservableCollection<Chat> CurrentChatList
        => IsCandidateMode ? FilteredChats : Chats;

    public async Task LoadAsync()
    {
        await RunSafelyAsync(async () =>
        {
            Chats.Clear();
            FilteredChats.Clear();
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

            if (IsCandidateMode)
            {
                ApplyTabFilter();
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

            List<object> results = new();

            if (IsCandidateMode)
            {
                if (ActiveTab == "Users")
                {
                    var users = await chatService.SearchUsersAsync(SearchQuery);
                    results.AddRange(users.Where(user => user.UserId != session.UserId));

                    var matchingChats = FindUserTabMatchingChats(FilteredChats, SearchQuery);
                    foreach (var chat in matchingChats)
                    {
                        results.Insert(0, chat);
                    }
                }
                else
                {
                    var companies = await chatService.SearchCompaniesAsync(SearchQuery);
                    results.AddRange(companies);

                    var matchingChats = FindCompanyTabMatchingChats(FilteredChats, SearchQuery);
                    foreach (var chat in matchingChats)
                    {
                        results.Insert(0, chat);
                    }
                }
            }
            else if (IsCompanyMode)
            {
                var users = await chatService.SearchUsersAsync(SearchQuery);
                results.AddRange(users);

                var matchingChats = FindCompanyModeMatchingChats(Chats, SearchQuery);
                foreach (var chat in matchingChats)
                {
                    results.Insert(0, chat);
                }
            }

            foreach (var result in results)
            {
                if (result is Chat chatResult)
                {
                    SearchResults.Add(ContactSearchResultViewModel.ForChat(chatResult));
                    continue;
                }

                if (result is User userResult)
                {
                    SearchResults.Add(ContactSearchResultViewModel.ForUser(userResult));
                    continue;
                }

                if (result is Company companyResult)
                {
                    SearchResults.Add(ContactSearchResultViewModel.ForCompany(companyResult));
                }
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
            Chat chat;

            if (result.Kind == ContactSearchResultKind.Chat && result.Chat is not null)
            {
                chat = result.Chat;
            }
            else if (result.Kind == ContactSearchResultKind.User)
            {
                if (IsCompanyMode && session.CompanyId is int companyId)
                {
                    var createdChat = await chatService.FindOrCreateUserCompanyChatAsync(result.Id, companyId);
                    if (createdChat is null)
                    {
                        return;
                    }

                    chat = createdChat;
                }
                else
                {
                    var createdChat = await chatService.FindOrCreateUserChatAsync(session.UserId, result.Id);
                    if (createdChat is null)
                    {
                        return;
                    }

                    chat = createdChat;
                }
            }
            else if (result.Kind == ContactSearchResultKind.Company)
            {
                var createdChat = await chatService.FindOrCreateUserCompanyChatAsync(session.UserId, result.Id);
                if (createdChat is null)
                {
                    return;
                }

                chat = createdChat;
            }
            else
            {
                return;
            }

            var oldChat = FindChatById(Chats, chat.ChatId);
            if (oldChat is not null)
            {
                Chats.Remove(oldChat);
            }

            Chats.Insert(0, chat);

            if (IsCandidateMode)
            {
                ActiveTab = chat.SecondUser != null ? "Users" : "Company";
                ApplyTabFilter();

                var oldFilteredChat = FindChatById(FilteredChats, chat.ChatId);
                if (oldFilteredChat is not null)
                {
                    FilteredChats.Remove(oldFilteredChat);
                }

                FilteredChats.Insert(0, chat);
            }

            SelectChat(chat);

            SearchQuery = string.Empty;
            SearchResults.Clear();
        });
    }

    public void StartCompanyChat(int companyId, int? jobId)
    {
        if (!IsCandidateMode)
        {
            return;
        }

        _ = RunSafelyAsync(async () =>
        {
            var chat = await chatService.FindOrCreateUserCompanyChatAsync(session.UserId, companyId, jobId);
            if (chat is null)
            {
                return;
            }

            var oldChat = FindChatById(Chats, chat.ChatId);
            if (oldChat is not null)
            {
                Chats.Remove(oldChat);
            }

            Chats.Insert(0, chat);

            ActiveTab = "Company";
            ApplyTabFilter();

            var oldFilteredChat = FindChatById(FilteredChats, chat.ChatId);
            if (oldFilteredChat is not null)
            {
                FilteredChats.Remove(oldFilteredChat);
            }

            FilteredChats.Insert(0, chat);

            SelectChat(chat);
            SearchQuery = string.Empty;
            SearchResults.Clear();
        });
    }

    private async Task LoadSelectedChatAsync()
    {
        Messages.Clear();
        if (SelectedChat is null)
        {
            UpdateVisibility();
            return;
        }

        await RunSafelyAsync(async () =>
        {
            var callerId = GetCallerId();
            var messages = await chatService.GetMessagesAsync(SelectedChat.ChatId, callerId);
            await chatService.MarkMessagesAsReadAsync(SelectedChat.ChatId, callerId);
            ApplyReadReceiptVisibility(messages);
            Messages.Clear();
            foreach (var message in messages)
            {
                message.SenderInitials = ResolveSenderInitials(message.Sender.SenderId);
                Messages.Add(new MessageDisplayViewModel(message));
            }

            if (SelectedChat.JobId.HasValue)
            {
                LinkedJob = await jobService.GetByIdAsync(SelectedChat.JobId.Value).ConfigureAwait(false);
            }
            else
            {
                LinkedJob = null;
            }

            UpdateVisibility();
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
            await chatService.SendMessageAsync(SelectedChat.ChatId, text, GetCallerId(), SelectedMessageType);
            SelectedMessageType = MessageType.Text;

            var selectedChatId = SelectedChat.ChatId;
            await RefreshInboxAndSelectedChatAsync();
            MoveChatToTop(Chats, selectedChatId);
            if (IsCandidateMode)
            {
                MoveChatToTop(FilteredChats, selectedChatId);
                var restoredSelection = FindChatById(FilteredChats, selectedChatId);
                if (restoredSelection is not null)
                {
                    SelectedChat = restoredSelection;
                }
            }
            else
            {
                var restoredSelection = FindChatById(Chats, selectedChatId);
                if (restoredSelection is not null)
                {
                    SelectedChat = restoredSelection;
                }
            }
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
            Chats.Remove(SelectedChat);
            FilteredChats.Remove(SelectedChat);
            SelectedChat = null;
            Messages.Clear();
            UpdateVisibility();
        });
    }

    private bool CanSend()
    {
        return SelectedChat is not null && !SelectedChat.IsBlocked && !string.IsNullOrWhiteSpace(MessageText);
    }

    public MessageType SelectedMessageType { get; private set; } = MessageType.Text;

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
            ErrorMessage = string.Empty;
            await action();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
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

    public void SwitchTab(string tabName)
    {
        ActiveTab = tabName;
        ApplyTabFilter();
        SelectedChat = null;
        Messages.Clear();
        UpdateVisibility();
    }

    public void SelectChat(Chat chat)
    {
        SelectedChat = chat;
    }

    public async Task RefreshInboxAndSelectedChatAsync()
    {
        var selectedChatId = SelectedChat?.ChatId;
        var callerId = GetCallerId();

        await RunSafelyAsync(async () =>
        {
            var latestChats = IsCompanyMode
                ? await chatService.GetChatsForCompanyAsync(callerId)
                : await chatService.GetChatsForUserAsync(callerId);

            var chatsChanged = MergeChats(latestChats);
            if (IsCandidateMode && chatsChanged)
            {
                ApplyTabFilter();
            }

            if (!selectedChatId.HasValue)
            {
                return;
            }

            var refreshedSelectedChat = FindChatById(Chats, selectedChatId.Value);
            if (refreshedSelectedChat is null)
            {
                SelectedChat = null;
                Messages.Clear();
                UpdateVisibility();
                return;
            }

            if (SelectedChat?.ChatId != refreshedSelectedChat.ChatId || !ReferenceEquals(SelectedChat, refreshedSelectedChat))
            {
                SelectedChat = refreshedSelectedChat;
            }

            var latestMessages = await chatService.GetMessagesAsync(refreshedSelectedChat.ChatId, callerId);

            var hasUnreadFromOtherParty = HasUnreadFromOtherParty(latestMessages, callerId);
            if (hasUnreadFromOtherParty)
            {
                await chatService.MarkMessagesAsReadAsync(refreshedSelectedChat.ChatId, callerId);
                foreach (var message in latestMessages.Where(message => message.Sender.SenderId != callerId))
                {
                    message.IsRead = true;
                }
            }

            ApplyReadReceiptVisibility(latestMessages);
            if (HaveMessagesChanged(latestMessages))
            {
                Messages.Clear();
                foreach (var message in latestMessages)
                {
                    message.SenderInitials = ResolveSenderInitials(message.Sender.SenderId);
                    Messages.Add(new MessageDisplayViewModel(message));
                }
            }

            if (SelectedChat.JobId.HasValue)
            {
                LinkedJob = await jobService.GetByIdAsync(SelectedChat.JobId.Value).ConfigureAwait(false);
            }
            else
            {
                LinkedJob = null;
            }

            UpdateVisibility();
        });
    }

    private void ApplyReadReceiptVisibility(IReadOnlyList<Message> messages)
    {
        var currentSenderId = GetCallerId();
        foreach (var message in messages)
        {
            message.ShowReadReceipt = false;
        }

        for (var index = messages.Count - 1; index >= 0; index--)
        {
            if (messages[index].Sender.SenderId == currentSenderId)
            {
                messages[index].ShowReadReceipt = true;
                break;
            }
        }
    }

    private void ApplyTabFilter()
    {
        if (!IsCandidateMode)
        {
            return;
        }

        var selectedChatId = SelectedChat?.ChatId;
        FilteredChats.Clear();

        var filtered = ActiveTab == "Users"
            ? GetChatsWithSecondUser(Chats)
            : GetChatsWithCompany(Chats);

        foreach (var chat in filtered)
        {
            FilteredChats.Add(chat);
        }

        if (selectedChatId.HasValue)
        {
            var restoredSelection = FindChatById(FilteredChats, selectedChatId.Value);
            if (restoredSelection is not null)
            {
                SelectedChat = restoredSelection;
            }
        }
    }

    private void UpdateVisibility()
    {
        if (SelectedChat is null)
        {
            ShowBlock = false;
            ShowUnblock = false;
            ShowGoToProfile = false;
            ShowGoToCompanyProfile = false;
            ShowGoToJobPost = false;
            return;
        }

        var callerId = GetCallerId();
        ShowBlock = !SelectedChat.IsBlocked;
        ShowUnblock = SelectedChat.IsBlocked && SelectedChat.BlockedByUser?.UserId == callerId;

        if (IsCompanyMode)
        {
            ShowGoToProfile = true;
            ShowGoToCompanyProfile = false;
        }
        else
        {
            ShowGoToProfile = SelectedChat.SecondUser != null;
            ShowGoToCompanyProfile = SelectedChat.CompanyId.HasValue;
        }

        ShowGoToJobPost = SelectedChat.JobId.HasValue;
    }

    private void SyncTabTogglesFromActiveTab()
    {
        isSyncingTabState = true;
        SetProperty(ref isUsersTabActive, ActiveTab == "Users", nameof(IsUsersTabActive));
        SetProperty(ref isCompaniesTabActive, ActiveTab == "Company", nameof(IsCompaniesTabActive));
        isSyncingTabState = false;
    }

    public void HandleAttachmentSelected(string filePath)
    {
        HandleAttachmentSelected(filePath, Path.GetExtension(filePath));
    }

    public void HandleAttachmentSelected(string filePath, string extension)
    {
        if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(extension))
        {
            ErrorMessage = "No file selected.";
            return;
        }

        var normalizedExtension = extension.ToLowerInvariant();
        if (normalizedExtension is ".jpg" or ".jpeg" or ".png")
        {
            SelectedMessageType = MessageType.Image;
        }
        else if (normalizedExtension is ".pdf" or ".doc" or ".docx")
        {
            SelectedMessageType = MessageType.File;
        }
        else
        {
            ErrorMessage = "Unsupported file type. Allowed: .jpg, .jpeg, .png, .pdf, .doc, .docx";
            return;
        }

        MessageText = filePath;
        _ = SendAsync();
    }

    public async Task DownloadAttachmentAsync(Message message, string targetPath)
    {
        if (message.Type != MessageType.File && message.Type != MessageType.Image)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            ErrorMessage = "No save location selected.";
            return;
        }

        try
        {
            await using var sourceStream = await chatService.OpenMessageAttachmentAsync(message.Content);
            await using var targetStream = File.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public void GoToProfile()
    {
        if (SelectedChat is null)
        {
            return;
        }

        var userId = SelectedChat.SecondUser?.UserId ?? SelectedChat.User.UserId;
        if (userId <= 0)
        {
            return;
        }

        ProfileNavigationRequested?.Invoke(userId);
    }

    public void GoToCompanyProfile()
    {
        if (SelectedChat?.CompanyId is null)
        {
            return;
        }

        CompanyNavigationRequested?.Invoke(SelectedChat.CompanyId.Value);
    }

    public void GoToJobPost()
    {
        if (SelectedChat?.JobId is null)
        {
            return;
        }

        JobNavigationRequested?.Invoke(SelectedChat.JobId.Value);
    }

    public event Action<int>? ProfileNavigationRequested;
    public event Action<int>? CompanyNavigationRequested;
    public event Action<int>? JobNavigationRequested;

    private static void MoveChatToTop(ObservableCollection<Chat> chats, int chatId)
    {
        var index = -1;
        for (var chatIndex = 0; chatIndex < chats.Count; chatIndex++)
        {
            if (chats[chatIndex].ChatId == chatId)
            {
                index = chatIndex;
                break;
            }
        }

        if (index > 0)
        {
            chats.Move(index, 0);
        }
    }

    private bool MergeChats(IReadOnlyList<Chat> latestChats)
    {
        var changed = false;
        var latestById = BuildChatDictionaryById(latestChats);
        var selectedChatId = SelectedChat?.ChatId;

        for (var existingChatIndex = Chats.Count - 1; existingChatIndex >= 0; existingChatIndex--)
        {
            var chatId = Chats[existingChatIndex].ChatId;
            if (!latestById.ContainsKey(chatId) && (!selectedChatId.HasValue || chatId != selectedChatId.Value))
            {
                Chats.RemoveAt(existingChatIndex);
                changed = true;
            }
        }

        for (var targetIndex = 0; targetIndex < latestChats.Count; targetIndex++)
        {
            var latest = latestChats[targetIndex];
            var currentIndex = -1;

            for (var currentChatIndex = 0; currentChatIndex < Chats.Count; currentChatIndex++)
            {
                if (Chats[currentChatIndex].ChatId == latest.ChatId)
                {
                    currentIndex = currentChatIndex;
                    break;
                }
            }

            if (currentIndex == -1)
            {
                Chats.Insert(targetIndex, latest);
                changed = true;
                continue;
            }

            if (IsChatDifferent(Chats[currentIndex], latest))
            {
                Chats[currentIndex] = latest;
                changed = true;
            }

            if (currentIndex != targetIndex)
            {
                Chats.Move(currentIndex, targetIndex);
                changed = true;
            }
        }

        return changed;
    }

    private static bool IsChatDifferent(Chat current, Chat updated)
    {
        return current.User.UserId != updated.User.UserId ||
               current.CompanyId != updated.CompanyId ||
               current.SecondUser?.UserId != updated.SecondUser?.UserId ||
               current.JobId != updated.JobId ||
               current.IsBlocked != updated.IsBlocked ||
               current.BlockedByUser?.UserId != updated.BlockedByUser?.UserId ||
               !Nullable.Equals(current.DeletedAtByUser, updated.DeletedAtByUser) ||
               !Nullable.Equals(current.DeletedAtBySecondParty, updated.DeletedAtBySecondParty) ||
               current.UnreadCount != updated.UnreadCount ||
               !string.Equals(current.OtherPartyName, updated.OtherPartyName, StringComparison.Ordinal) ||
               !string.Equals(current.LastMessage, updated.LastMessage, StringComparison.Ordinal) ||
               !string.Equals(current.LastMessageSnippet, updated.LastMessageSnippet, StringComparison.Ordinal) ||
               !string.Equals(current.LastMessageTime, updated.LastMessageTime, StringComparison.Ordinal);
    }

    private bool HaveMessagesChanged(IReadOnlyList<Message> latestMessages)
    {
        if (Messages.Count != latestMessages.Count)
        {
            return true;
        }

        for (var messageIndex = 0; messageIndex < latestMessages.Count; messageIndex++)
        {
            var current = Messages[messageIndex].Message;
            var latest = latestMessages[messageIndex];

            if (current.MessageId != latest.MessageId ||
                current.IsRead != latest.IsRead ||
                current.Content != latest.Content ||
                current.Timestamp != latest.Timestamp ||
                current.Sender.SenderId != latest.Sender.SenderId ||
                current.Type != latest.Type)
            {
                return true;
            }
        }

        return false;
    }

    private string ResolveSenderInitials(int senderId)
    {
        if (IsCandidateMode)
        {
            if (senderId == session.UserId)
            {
                return "U";
            }

            return "U";
        }

        if (IsCompanyMode)
        {
            if (session.CompanyId is int companyId && senderId == companyId)
            {
                return "C";
            }

            return "U";
        }

        if (session.DeveloperId is int developerId && senderId == developerId)
        {
            return "D";
        }

        return "U";
    }

    private static List<Chat> GetChatsWithSecondUser(IEnumerable<Chat> chats)
    {
        var result = new List<Chat>();
        foreach (var chat in chats)
        {
            if (chat.SecondUser != null)
            {
                result.Add(chat);
            }
        }

        return result;
    }

    private static List<Chat> GetChatsWithCompany(IEnumerable<Chat> chats)
    {
        var result = new List<Chat>();
        foreach (var chat in chats)
        {
            if (chat.CompanyId.HasValue)
            {
                result.Add(chat);
            }
        }

        return result;
    }

    private static Chat? FindChatById(IEnumerable<Chat> chats, int chatId)
    {
        foreach (var chat in chats)
        {
            if (chat.ChatId == chatId)
            {
                return chat;
            }
        }

        return null;
    }

    private static bool HasUnreadFromOtherParty(IReadOnlyList<Message> messages, int currentCallerId)
    {
        foreach (var message in messages)
        {
            if (message.Sender.SenderId != currentCallerId && !message.IsRead)
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<int, Chat> BuildChatDictionaryById(IReadOnlyList<Chat> chats)
    {
        var result = new Dictionary<int, Chat>();
        foreach (var chat in chats)
        {
            result[chat.ChatId] = chat;
        }

        return result;
    }

    private List<Chat> FindUserTabMatchingChats(IEnumerable<Chat> chats, string query)
    {
        var result = new List<Chat>();
        foreach (var chat in chats)
        {
            if (chat.SecondUser == null)
            {
                continue;
            }

            if (chat.OtherPartyName.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(chat);
            }
        }

        return result;
    }

    private List<Chat> FindCompanyTabMatchingChats(IEnumerable<Chat> chats, string query)
    {
        var result = new List<Chat>();
        foreach (var chat in chats)
        {
            if (!chat.CompanyId.HasValue)
            {
                continue;
            }

            if (chat.OtherPartyName.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(chat);
            }
        }

        return result;
    }

    private List<Chat> FindCompanyModeMatchingChats(IEnumerable<Chat> chats, string query)
    {
        var result = new List<Chat>();
        foreach (var chat in chats)
        {
            if (chat.User.UserId <= 0)
            {
                continue;
            }

            if (chat.OtherPartyName.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(chat);
            }
        }

        return result;
    }
}

public sealed class MessageDisplayViewModel
{
    public MessageDisplayViewModel(Message message)
    {
        Message = message;
        Timestamp = message.Timestamp.ToLocalTime().ToString("HH:mm");
    }

    public string Timestamp { get; }
    public Message Message { get; }
}

public sealed class ContactSearchResultViewModel
{
    private ContactSearchResultViewModel(ContactSearchResultKind kind, int id, string displayName, string secondaryText, Chat? chat)
    {
        Kind = kind;
        Id = id;
        DisplayName = displayName;
        SecondaryText = secondaryText;
        Chat = chat;
    }

    public ContactSearchResultKind Kind { get; }
    public int Id { get; }
    public string DisplayName { get; }
    public string SecondaryText { get; }
    public Chat? Chat { get; }

    public static ContactSearchResultViewModel ForCompany(Company company)
    {
        return new ContactSearchResultViewModel(ContactSearchResultKind.Company, company.CompanyId, company.CompanyName, "Company", null);
    }

    public static ContactSearchResultViewModel ForUser(User user)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = $"User {user.UserId}";
        }

        return new ContactSearchResultViewModel(ContactSearchResultKind.User, user.UserId, fullName, user.Email, null);
    }

    public static ContactSearchResultViewModel ForChat(Chat chat)
    {
        return new ContactSearchResultViewModel(ContactSearchResultKind.Chat, chat.ChatId, chat.OtherPartyName, chat.LastMessageSnippet, chat);
    }
}

public enum ContactSearchResultKind
{
    Chat,
    User,
    Company,
}
