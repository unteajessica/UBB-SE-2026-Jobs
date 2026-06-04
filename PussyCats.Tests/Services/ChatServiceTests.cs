using NSubstitute;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Chats;
using PussyCats.Library.Repositories.Messages;
using PussyCats.Library.Services.ChatService;
using PussyCats.Library.Services.FileStorage;
using PussyCats.Library.Services.Users;
using PussyCats.Library.Services.CompanyService;

namespace PussyCats.Tests.Services
{

    public class ChatServiceTests
    {
        private readonly IChatRepository chatRepository = Substitute.For<IChatRepository>();
        private readonly IMessageRepository messageRepository = Substitute.For<IMessageRepository>();
        private readonly IUserService userService = Substitute.For<IUserService>();
        private readonly ICompanyService companyService = Substitute.For<ICompanyService>();
        private readonly ILocalFileStorageService fileStorage = Substitute.For<ILocalFileStorageService>();


        public required ChatService chatService;

        public ChatServiceTests()
        {
            chatService = new(chatRepository, messageRepository, userService, companyService, fileStorage);
        }

        #region FindOrCreateUser....
        [Fact]
        public async Task FindOrCreateUserCompanyChatAsync_NoExistingChat_CreatesAndReturnsNewChat()
        {
            var company = new Company();
            var user = new User { UserId = 1 };
            var newChat = new Chat();

            chatRepository.FindUserCompanyChatAsync(1, company, null, Arg.Any<CancellationToken>())
                .Returns((Chat?)null);
            chatRepository.AddAsync(Arg.Any<Chat>(), Arg.Any<CancellationToken>())
                .Returns(newChat);
            userService.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<User>>(new List<User> { user })); 
            
            var createdChat = await chatService.FindOrCreateUserCompanyChatAsync(1, company);

            Assert.Same(newChat, createdChat);
        }

        [Fact]
        public async Task FindOrCreateUserCompanyChatAsync_ExistingChat_ReturnsExistingChatWithDeletionFlagsCleared()
        {
            var existingChat = new Chat {ChatId = 1, DeletedAtByUser = DateTime.UtcNow, DeletedAtBySecondParty = DateTime.UtcNow };
            var company = new Company();

            chatRepository.FindUserCompanyChatAsync(1, company, null, Arg.Any<CancellationToken>())
                .Returns(existingChat);

            var returnedChat = await chatService.FindOrCreateUserCompanyChatAsync(1, company);

            Assert.Equal(existingChat.ChatId, returnedChat.ChatId);
            Assert.Null(returnedChat!.DeletedAtByUser);
            Assert.Null(returnedChat!.DeletedAtBySecondParty);
        }

        [Fact]
        public async Task FindOrCreateUserChatAsync_NoExistingChat_CreatesAndReturnsNewChat()
        {
            var user1 = new User { UserId = 1 };
            var user2 = new User { UserId = 2 };
            var newChat = new Chat();

            chatRepository.FindUserUserChatAsync(1, 2, Arg.Any<CancellationToken>())
                .Returns((Chat?)null);
            chatRepository.AddAsync(Arg.Any<Chat>(), Arg.Any<CancellationToken>())
                .Returns(newChat);
            userService.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<User>>(new List<User> { user1, user2 }));


            var createdChat = await chatService.FindOrCreateUserChatAsync(1, 2);


            Assert.Same(newChat, createdChat);
        }

        [Fact]
        public async Task FindOrCreateUserChatAsync_ExistingChat_ReturnsExistingChatWithDeletionFlagsCleared()
        {
            var existingChat = new Chat { ChatId = 1, DeletedAtByUser = DateTime.UtcNow, DeletedAtBySecondParty = DateTime.UtcNow };

            chatRepository.FindUserUserChatAsync(1, 2, Arg.Any<CancellationToken>())
                .Returns(existingChat);

            var returnedChat = await chatService.FindOrCreateUserChatAsync(1, 2);

            Assert.Equal(existingChat.ChatId, returnedChat!.ChatId);
            Assert.Null(returnedChat!.DeletedAtByUser);
            Assert.Null(returnedChat!.DeletedAtBySecondParty);
        }
        #endregion

        #region GetChatsFor....
        [Fact]
        public async Task GetChatsForUserAsync_ChatBlockedByOtherParty_ExcludesChat()
        {
            var userId = 1;
            var blockedChat = new Chat
            {
                IsBlocked = true,
                BlockedByUser = new User { UserId = 2 }, 
                User = new User { UserId = userId }
            };

            chatRepository.GetForUserAsync(userId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Chat>>(new List<Chat> { blockedChat }));

            var result = await chatService.GetChatsForUserAsync(userId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetChatsForUserAsync_ChatBlockedByCaller_IncludesChat()
        {
            var userId = 1;
            var blockedBySelfChat = new Chat
            {
                IsBlocked = true,
                BlockedByUser = new User { UserId = userId },
                User = new User { UserId = userId }
            };

            chatRepository.GetForUserAsync(userId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Chat>>(new List<Chat> { blockedBySelfChat }));

            var result = await chatService.GetChatsForUserAsync(userId);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetChatsForUserAsync_ChatDeletedByUser_ExcludesChat
            ()
        {
            var userId = 1;
            var deletedChat = new Chat
            {
                IsBlocked = false,
                User = new User { UserId = userId },
                DeletedAtByUser = DateTime.UtcNow
            };

            chatRepository.GetForUserAsync(userId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Chat>>(new List<Chat> { deletedChat }));

            var result = await chatService.GetChatsForUserAsync(userId);

            Assert.Empty(result);
        }
        [Fact]
        public async Task GetChatsForCompanyAsync_ChatDeletedByCompany_ExcludesChat()
        {
            var companyId = 1;
            var deletedChat = new Chat
            {
                IsBlocked = false,
                User = new User { UserId = 99 },
                DeletedAtBySecondParty = DateTime.UtcNow
            };

            chatRepository.GetForCompanyAsync(companyId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Chat>>(new List<Chat> { deletedChat }));

            var result = await chatService.GetChatsForCompanyAsync(companyId);

            Assert.Empty(result);
        }
        #endregion

        #region GetMessagesAsync

        [Fact]
        public async Task GetMessagesAsync_ChatNotFound_ThrowsKeyNotFoundException()
        {
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((Chat?)null);

            var action = async () => await chatService.GetMessagesAsync(1, callerId: 1);

            await Assert.ThrowsAsync<KeyNotFoundException>(action);
        }

        [Fact]
        public async Task GetMessagesAsync_CallerIsNotParticipant_ThrowsUnauthorizedAccessException()
        {
            var chat = new Chat { User = new User { UserId = 2 }, SecondUser = new User { UserId = 3 } };

            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns(chat);

            var act = async () => await chatService.GetMessagesAsync(1, callerId: 99);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
        }

        [Fact]
        public async Task GetMessagesAsync_CallerHasDeletedAt_ExcludesMessagesBeforeDeletion()
        {
            var deletedAt = DateTime.UtcNow;
            var callerId = 1;
            var chat = new Chat { User = new User { UserId = callerId }, DeletedAtByUser = deletedAt };
            var oldMessage = new Message { Timestamp = deletedAt.AddMinutes(-1), Sender = new MessageSender { SenderId = 2 }, Chat = new Chat { ChatId = 1 } };
            var newMessage = new Message { Timestamp = deletedAt.AddMinutes(1), Sender = new MessageSender { SenderId = 2 }, Chat = new Chat { ChatId = 1 } };

            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);
            messageRepository.GetForChatAsync(1, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Message>>(new List<Message> { oldMessage, newMessage }));

            var result = await chatService.GetMessagesAsync(1, callerId);

            Assert.Equal(newMessage.Timestamp, Assert.Single(result).Timestamp);
        }

        [Fact]
        public async Task GetMessagesAsync_ValidRequest_SetsMessagesMetadataCorrectly()
        {
            var callerId = 1;
            var chat = new Chat { User = new User { UserId = callerId } };
            var ownMessage = new Message { Sender = new MessageSender { SenderId = callerId }, Chat = new Chat { ChatId = 1 }, Timestamp = DateTime.UtcNow };
            var otherMessage = new Message { Sender = new MessageSender { SenderId = 2 }, Chat = new Chat { ChatId = 1 }, Timestamp = DateTime.UtcNow };

            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);
            messageRepository.GetForChatAsync(1, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Message>>(new List<Message> { ownMessage, otherMessage }));

            var result = await chatService.GetMessagesAsync(1, callerId);

            Assert.True(result[0].ShowReadReceipt);
            Assert.Equal("Me", result[0].SenderInitials);
            Assert.False(result[1].ShowReadReceipt);
            Assert.Equal("Them", result[1].SenderInitials);
        }

        #endregion GetMessagesAsync

        #region SearchCompaniesAsync and SearchUsersAsync
        [Fact]
        public async Task SearchCompaniesAsync_EmptySearchTerm_ReturnsEmpty()
        {
            var result = await chatService.SearchCompaniesAsync("   ");

            Assert.Empty(result);
            await companyService.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SearchCompaniesAsync_MatchingTerm_ReturnsFilteredCompanies()
        {
            var availableCompanies = new List<Company>
            {
                new() { CompanyName = "Acme Corp" },
                new() { CompanyName = "Other Company" }
            };

            companyService.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Company>>(availableCompanies));

            var result = await chatService.SearchCompaniesAsync("acme");

            Assert.Equal("Acme Corp", Assert.Single(result).CompanyName);
        }

        [Fact]
        public async Task SearchCompaniesAsync_MoreThanMaxResults_ReturnsOnlyMaxResults()
        {
            var companies = Enumerable.Range(1, 15)
                .Select(companyNumber => new Company { CompanyName = $"Company {companyNumber}" })
                .ToList();

            companyService.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Company>>(companies));

            var result = await chatService.SearchCompaniesAsync("Company");

            Assert.Equal(10, result.Count());
        }

        [Fact]
        public async Task SearchUsersAsync_EmptySearchTerm_ReturnsEmpty()
        {
            var result = await chatService.SearchUsersAsync("   ");

            Assert.Empty(result);
            await userService.DidNotReceive().GetAllAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SearchUsersAsync_MatchingTerm_ReturnsFilteredUsers()
        {
            var users = new List<User>
            {
                new() { FirstName = "Peter", LastName = "Pann" },
                new() { FirstName = "Tink", LastName = "Smith" }
            };

            userService.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<User>>(users));

            var result = await chatService.SearchUsersAsync("peter");

            Assert.Equal("Peter", Assert.Single(result).FirstName);
        }

        [Fact]
        public async Task SearchUsersAsync_MoreThanMaxResults_ReturnsOnlyMaxResults()
        {
            var users = Enumerable.Range(1, 15)
                .Select(userNumber => new User { FirstName = "Peter", LastName = $"Pann {userNumber}" })
                .ToList();

            userService.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<User>>(users));

            var result = await chatService.SearchUsersAsync("peter");

            Assert.Equal(10, result.Count());
        }
        #endregion


        #region SendMessage
        [Fact]
        public async Task SendMessageAsync_EmptyOrNullContent_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => chatService.SendMessageAsync(1, "   ", 1, MessageType.Text));
        }

        [Fact]
        public async Task SendMessageAsync_ChatNotFound_ThrowsKeyNotFoundException()
        {
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Chat?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => chatService.SendMessageAsync(1, "hello", 1, MessageType.Text));
        }

        [Fact]
        public async Task SendMessageAsync_CallerIsNotParticipant_ThrowsUnauthorizedAccessException()
        {
            var chat = new Chat { User = new User { UserId = 2 }, SecondUser = new User { UserId = 3 } };
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => chatService.SendMessageAsync(1, "hello", 789, MessageType.Text));
        }

        [Fact]
        public async Task SendMessageAsync_BlockedChat_ThrowsInvalidOperationException()
        {
            var chat = new Chat { IsBlocked = true, User = new User { UserId = 1 } };
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await Assert.ThrowsAsync<InvalidOperationException>(() => chatService.SendMessageAsync(1, "hello", 1, MessageType.Text));
        }

        [Fact]
        public async Task SendMessageAsync_TextMessageExceedsMaxLength_ThrowsArgumentException()
        {
            var chat = new Chat { User = new User { UserId = 1 } };
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await Assert.ThrowsAsync<ArgumentException>(() => chatService.SendMessageAsync(1, new string('a', 2001), 1, MessageType.Text));
        }

        [Fact]
        public async Task SendMessageAsync_ValidTextMessage_AddsMessageToRepository()
        {
            var chat = new Chat { User = new User { UserId = 1 } };
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.SendMessageAsync(1, "hello", 1, MessageType.Text);

            await messageRepository.Received(1).AddAsync(
                Arg.Is<Message>(message => message.Content == "hello" && message.Type == MessageType.Text),
                Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(".jpg")]
        [InlineData(".jpeg")]
        [InlineData(".png")]
        [InlineData(".PNG")]
        public async Task SendMessageAsync_ValidImageMessage_StoresAttachmentAndAddsMessage(string imageExtension)
        {
            var chat = new Chat { User = new User { UserId = 1 } };
            var tempFile = Path.GetTempFileName();
            var imagePath = Path.ChangeExtension(tempFile, imageExtension);
            File.Move(tempFile, imagePath);
            await File.WriteAllBytesAsync(imagePath, new byte[100]);

            try
            {
                chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);
                fileStorage.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns("stored/path.jpg");

                await chatService.SendMessageAsync(1, imagePath, 1, MessageType.Image);

                await messageRepository.Received(1).AddAsync(
                    Arg.Is<Message>(message => message.Content == "stored/path.jpg" && message.OriginalFileName == Path.GetFileName(imagePath)),
                    Arg.Any<CancellationToken>());
            }
            finally
            {
                File.Delete(imagePath);
            }
        }

        [Theory]
        [InlineData(".pdf")]
        [InlineData(".docx")]
        [InlineData(".doc")]
        [InlineData(".DOC")]
        public async Task SendMessageAsync_ValidFileAttachmentMessage_StoresAttachmentAndAddsMessage(string fileExtension)
        {
            var chat = new Chat { User = new User { UserId = 1 } };
            var tempFile = Path.GetTempFileName();
            var sentAttachmentPath = Path.ChangeExtension(tempFile, fileExtension);
            File.Move(tempFile, sentAttachmentPath);
            await File.WriteAllBytesAsync(sentAttachmentPath, new byte[100]);

            try
            {
                chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);
                fileStorage.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns("stored/path.jpg");

                await chatService.SendMessageAsync(1, sentAttachmentPath, 1, MessageType.File);

                await messageRepository.Received(1).AddAsync(
                    Arg.Is<Message>(message => message.Content == "stored/path.jpg" && message.OriginalFileName == Path.GetFileName(sentAttachmentPath)),
                    Arg.Any<CancellationToken>());
            }
            finally
            {
                File.Delete(sentAttachmentPath);
            }
        }
        #endregion

        [Fact]
        public async Task OpenMessageAttachmentAsync_EmptyPath_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => chatService.OpenMessageAttachmentAsync("   "));
        }

        [Fact]
        public async Task OpenMessageAttachmentAsync_ValidPath_ReturnsStreamFromStorage()
        {
            var stream = new MemoryStream();
            fileStorage.OpenReadAsync("path/file.jpg", Arg.Any<CancellationToken>())
                .Returns(stream);

            var result = await chatService.OpenMessageAttachmentAsync("path/file.jpg");

            Assert.Same(stream, result);
        }

        #region BlockChatAsync and UnblockChatAsync

        [Fact]
        public async Task BlockChatAsync_ChatNotFound_ThrowsKeyNotFoundException()
        {
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Chat?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => chatService.BlockChatAsync(1, blockerId: 1));
        }

        [Fact]
        public async Task BlockChatAsync_CallerIsNotParticipant_ThrowsUnauthorizedAccessException()
        {
            var chat = new Chat { User = new User { UserId = 2 }, SecondUser = new User { UserId = 3 } };
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => chatService.BlockChatAsync(1, blockerId: 99));
        }

        [Fact]
        public async Task BlockChatAsync_ValidRequest_BlocksChatAndUpdatesRepository()
        {
            var chat = new Chat { User = new User { UserId = 1 } };
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.BlockChatAsync(1, blockerId: 1);

            Assert.True(chat.IsBlocked);
            Assert.Equal(1, chat.BlockedByUser!.UserId);
            chatRepository.Received(1).UpdateAsync(chat, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UnblockChatAsync_ChatNotFound_ThrowsKeyNotFoundException()
        {
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Chat?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => chatService.UnblockChatAsync(1, unblockerId: 1));
        }


        [Fact]
        public async Task UnblockChatAsync_CallerIsNotBlocker_ThrowsUnauthorizedAccessException()
        {
            var chat = new Chat { User = new User { UserId = 1 }, SecondUser = new User { UserId = 2 }, BlockedByUser = new User { UserId = 2 } };
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => chatService.UnblockChatAsync(1, unblockerId: 1));
        }

        [Fact]
        public async Task UnblockChatAsync_ValidRequest_UnblocksChatAndUpdatesRepository()
        {
            var chat = new Chat { User = new User { UserId = 1 }, BlockedByUser = new User { UserId = 1 }, IsBlocked = true };
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.UnblockChatAsync(1, unblockerId: 1);

            Assert.False(chat.IsBlocked);
            Assert.Null(chat.BlockedByUser);
            chatRepository.Received(1).UpdateAsync(chat, Arg.Any<CancellationToken>());
        }

        #endregion

        [Fact]
        public async Task DeleteChatAsync_ChatNotFound_ThrowsKeyNotFoundException()
        {
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Chat?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => chatService.DeleteChatAsync(1, callerId: 1));
        }

        [Fact]
        public async Task DeleteChatAsync_CallerIsNotParticipant_ThrowsUnauthorizedAccessException()
        {
            var chat = new Chat { User = new User { UserId = 2 }, SecondUser = new User { UserId = 3 } };
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => chatService.DeleteChatAsync(1, callerId: 99));
        }

        [Fact]
        public async Task DeleteChatAsync_CallerIsUser_SetsDeletedAtByUser()
        {
            var chat = new Chat { User = new User { UserId = 1 } };
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.DeleteChatAsync(1, callerId: 1);

            Assert.NotNull(chat.DeletedAtByUser);
            Assert.Null(chat.DeletedAtBySecondParty);
            chatRepository.Received(1).UpdateAsync(chat, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DeleteChatAsync_CallerIsSecondParty_SetsDeletedAtBySecondParty()
        {
            var chat = new Chat { User = new User { UserId = 1 }, SecondUser = new User { UserId = 2 } };
            chatRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.DeleteChatAsync(1, callerId: 2);

            Assert.NotNull(chat.DeletedAtBySecondParty);
            Assert.Null(chat.DeletedAtByUser);
            chatRepository.Received(1).UpdateAsync(chat, Arg.Any<CancellationToken>());
        }
    }
}