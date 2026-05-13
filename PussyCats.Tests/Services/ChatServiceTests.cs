using FluentAssertions;
using NSubstitute;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Repositories.Chats;
using PussyCats.Library.Repositories.Messages;

namespace PussyCats.Tests.Services
{

    public class ChatServiceTests
    {
        private readonly IChatRepository chatRepo = Substitute.For<IChatRepository>();
        private readonly IMessageRepository messageRepo = Substitute.For<IMessageRepository>();
        private readonly IUserService userService = Substitute.For<IUserService>();
        private readonly ICompanyService companyService = Substitute.For<ICompanyService>();
        private readonly ILocalFileStorageService fileStorage = Substitute.For<ILocalFileStorageService>();


        public required ChatService chatService;

        public ChatServiceTests()
        {
            chatService = new(chatRepo, messageRepo, userService, companyService, fileStorage);
        }

        #region FindOrCreateUser....
        [Fact]
        public async Task FindOrCreateUserCompanyChatAsync_NoExistingChat_CreatesAndReturnsNewChat()
        {
            var company = new Company();
            var user = new User { UserId = 1 };
            var newChat = new Chat();

            chatRepo.FindUserCompanyChatAsync(1, company, null, Arg.Any<CancellationToken>())
                .Returns((Chat?)null);
            chatRepo.AddAsync(Arg.Any<Chat>(), Arg.Any<CancellationToken>())
                .Returns(newChat);
            userService.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<User>>(new List<User> { user })); 
            
            var createdChat = await chatService.FindOrCreateUserCompanyChatAsync(1, company);

            createdChat.Should().BeSameAs(newChat);
        }

        [Fact]
        public async Task FindOrCreateUserCompanyChatAsync_ExistingChat_ReturnsExistingChatWithDeletionFlagsCleared()
        {
            var existingChat = new Chat {ChatId = 1, DeletedAtByUser = DateTime.UtcNow, DeletedAtBySecondParty = DateTime.UtcNow };
            var company = new Company();

            chatRepo.FindUserCompanyChatAsync(1, company, null, Arg.Any<CancellationToken>())
                .Returns(existingChat);

            var returnedChat = await chatService.FindOrCreateUserCompanyChatAsync(1, company);

            returnedChat.ChatId.Should().Be(existingChat.ChatId);
            returnedChat!.DeletedAtByUser.Should().BeNull();
            returnedChat!.DeletedAtBySecondParty.Should().BeNull();
        }

        [Fact]
        public async Task FindOrCreateUserChatAsync_NoExistingChat_CreatesAndReturnsNewChat()
        {
            var user1 = new User { UserId = 1 };
            var user2 = new User { UserId = 2 };
            var newChat = new Chat();

            chatRepo.FindUserUserChatAsync(1, 2, Arg.Any<CancellationToken>())
                .Returns((Chat?)null);
            chatRepo.AddAsync(Arg.Any<Chat>(), Arg.Any<CancellationToken>())
                .Returns(newChat);
            userService.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<User>>(new List<User> { user1, user2 }));


            var createdChat = await chatService.FindOrCreateUserChatAsync(1, 2);


            createdChat.Should().BeSameAs(newChat);
        }

        [Fact]
        public async Task FindOrCreateUserChatAsync_ExistingChat_ReturnsExistingChatWithDeletionFlagsCleared()
        {
            var existingChat = new Chat { ChatId = 1, DeletedAtByUser = DateTime.UtcNow, DeletedAtBySecondParty = DateTime.UtcNow };

            chatRepo.FindUserUserChatAsync(1, 2, Arg.Any<CancellationToken>())
                .Returns(existingChat);

            var returnedChat = await chatService.FindOrCreateUserChatAsync(1, 2);

            returnedChat!.ChatId.Should().Be(existingChat.ChatId);
            returnedChat!.DeletedAtByUser.Should().BeNull();
            returnedChat!.DeletedAtBySecondParty.Should().BeNull();
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

            chatRepo.GetForUserAsync(userId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Chat>>(new List<Chat> { blockedChat }));

            var result = await chatService.GetChatsForUserAsync(userId);

            result.Should().BeEmpty();
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

            chatRepo.GetForUserAsync(userId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Chat>>(new List<Chat> { blockedBySelfChat }));

            var result = await chatService.GetChatsForUserAsync(userId);

            result.Should().ContainSingle();
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

            chatRepo.GetForUserAsync(userId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Chat>>(new List<Chat> { deletedChat }));

            var result = await chatService.GetChatsForUserAsync(userId);

            result.Should().BeEmpty();
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

            chatRepo.GetForCompanyAsync(companyId, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Chat>>(new List<Chat> { deletedChat }));

            var result = await chatService.GetChatsForCompanyAsync(companyId);

            result.Should().BeEmpty();
        }
        #endregion

        #region GetMessagesAsync

        [Fact]
        public async Task GetMessagesAsync_ChatNotFound_ThrowsKeyNotFoundException()
        {
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((Chat?)null);

            var action = async () => await chatService.GetMessagesAsync(1, callerId: 1);

            await action.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetMessagesAsync_CallerIsNotParticipant_ThrowsUnauthorizedAccessException()
        {
            var chat = new Chat { User = new User { UserId = 2 }, SecondUser = new User { UserId = 3 } };

            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns(chat);

            var act = async () => await chatService.GetMessagesAsync(1, callerId: 99);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetMessagesAsync_CallerHasDeletedAt_ExcludesMessagesBeforeDeletion()
        {
            var deletedAt = DateTime.UtcNow;
            var callerId = 1;
            var chat = new Chat { User = new User { UserId = callerId }, DeletedAtByUser = deletedAt };
            var oldMessage = new Message { Timestamp = deletedAt.AddMinutes(-1), Sender = new MessageSender { SenderId = 2 }, Chat = new Chat { ChatId = 1 } };
            var newMessage = new Message { Timestamp = deletedAt.AddMinutes(1), Sender = new MessageSender { SenderId = 2 }, Chat = new Chat { ChatId = 1 } };

            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);
            messageRepo.GetForChatAsync(1, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Message>>(new List<Message> { oldMessage, newMessage }));

            var result = await chatService.GetMessagesAsync(1, callerId);

            result.Should().ContainSingle()
                .Which.Timestamp.Should().Be(newMessage.Timestamp);
        }

        [Fact]
        public async Task GetMessagesAsync_ValidRequest_SetsMessagesMetadataCorrectly()
        {
            var callerId = 1;
            var chat = new Chat { User = new User { UserId = callerId } };
            var ownMessage = new Message { Sender = new MessageSender { SenderId = callerId }, Chat = new Chat { ChatId = 1 }, Timestamp = DateTime.UtcNow };
            var otherMessage = new Message { Sender = new MessageSender { SenderId = 2 }, Chat = new Chat { ChatId = 1 }, Timestamp = DateTime.UtcNow };

            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);
            messageRepo.GetForChatAsync(1, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Message>>(new List<Message> { ownMessage, otherMessage }));

            var result = await chatService.GetMessagesAsync(1, callerId);

            result[0].ShowReadReceipt.Should().BeTrue();
            result[0].SenderInitials.Should().Be("Me");
            result[1].ShowReadReceipt.Should().BeFalse();
            result[1].SenderInitials.Should().Be("Them");
        }

        #endregion GetMessagesAsync

        #region SearchCompaniesAsync and SearchUsersAsync
        [Fact]
        public async Task SearchCompaniesAsync_EmptySearchTerm_ReturnsEmpty()
        {
            var result = await chatService.SearchCompaniesAsync("   ");

            result.Should().BeEmpty();
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

            result.Should().ContainSingle()
                .Which.CompanyName.Should().Be("Acme Corp");
        }

        [Fact]
        public async Task SearchCompaniesAsync_MoreThanMaxResults_ReturnsOnlyMaxResults()
        {
            var companies = Enumerable.Range(1, 15)
                .Select(i => new Company { CompanyName = $"Company {i}" })
                .ToList();

            companyService.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Company>>(companies));

            var result = await chatService.SearchCompaniesAsync("Company");

            result.Should().HaveCount(10);
        }

        [Fact]
        public async Task SearchUsersAsync_EmptySearchTerm_ReturnsEmpty()
        {
            var result = await chatService.SearchUsersAsync("   ");

            result.Should().BeEmpty();
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

            result.Should().ContainSingle()
                .Which.FirstName.Should().Be("Peter");
        }

        [Fact]
        public async Task SearchUsersAsync_MoreThanMaxResults_ReturnsOnlyMaxResults()
        {
            var users = Enumerable.Range(1, 15)
                .Select(i => new User { FirstName = "Peter", LastName = $"Pann {i}" })
                .ToList();

            userService.GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<User>>(users));

            var result = await chatService.SearchUsersAsync("peter");

            result.Should().HaveCount(10);
        }
        #endregion


        #region SendMessage
        [Fact]
        public async Task SendMessageAsync_EmptyOrNullContent_ThrowsArgumentException()
        {
            await chatService.Invoking(s => s.SendMessageAsync(1, "   ", 1, MessageType.Text))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task SendMessageAsync_ChatNotFound_ThrowsKeyNotFoundException()
        {
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Chat?)null);

            await chatService.Invoking(s => s.SendMessageAsync(1, "hello", 1, MessageType.Text))
                .Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task SendMessageAsync_CallerIsNotParticipant_ThrowsUnauthorizedAccessException()
        {
            var chat = new Chat { User = new User { UserId = 2 }, SecondUser = new User { UserId = 3 } };
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.Invoking(s => s.SendMessageAsync(1, "hello", 789, MessageType.Text))
                .Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task SendMessageAsync_BlockedChat_ThrowsInvalidOperationException()
        {
            var chat = new Chat { IsBlocked = true, User = new User { UserId = 1 } };
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.Invoking(s => s.SendMessageAsync(1, "hello", 1, MessageType.Text))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SendMessageAsync_TextMessageExceedsMaxLength_ThrowsArgumentException()
        {
            var chat = new Chat { User = new User { UserId = 1 } };
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.Invoking(s => s.SendMessageAsync(1, new string('a', 2001), 1, MessageType.Text))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task SendMessageAsync_ValidTextMessage_AddsMessageToRepository()
        {
            var chat = new Chat { User = new User { UserId = 1 } };
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.SendMessageAsync(1, "hello", 1, MessageType.Text);

            await messageRepo.Received(1).AddAsync(
                Arg.Is<Message>(m => m.Content == "hello" && m.Type == MessageType.Text),
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
                chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);
                fileStorage.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns("stored/path.jpg");

                await chatService.SendMessageAsync(1, imagePath, 1, MessageType.Image);

                await messageRepo.Received(1).AddAsync(
                    Arg.Is<Message>(m => m.Content == "stored/path.jpg" && m.OriginalFileName == Path.GetFileName(imagePath)),
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
                chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);
                fileStorage.SaveFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns("stored/path.jpg");

                await chatService.SendMessageAsync(1, sentAttachmentPath, 1, MessageType.File);

                await messageRepo.Received(1).AddAsync(
                    Arg.Is<Message>(m => m.Content == "stored/path.jpg" && m.OriginalFileName == Path.GetFileName(sentAttachmentPath)),
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
            await chatService.Invoking(s => s.OpenMessageAttachmentAsync("   "))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task OpenMessageAttachmentAsync_ValidPath_ReturnsStreamFromStorage()
        {
            var stream = new MemoryStream();
            fileStorage.OpenReadAsync("path/file.jpg", Arg.Any<CancellationToken>())
                .Returns(stream);

            var result = await chatService.OpenMessageAttachmentAsync("path/file.jpg");

            result.Should().BeSameAs(stream);
        }

        #region BlockChatAsync and UnblockChatAsync

        [Fact]
        public async Task BlockChatAsync_ChatNotFound_ThrowsKeyNotFoundException()
        {
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Chat?)null);

            await chatService.Invoking(s => s.BlockChatAsync(1, blockerId: 1))
                .Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task BlockChatAsync_CallerIsNotParticipant_ThrowsUnauthorizedAccessException()
        {
            var chat = new Chat { User = new User { UserId = 2 }, SecondUser = new User { UserId = 3 } };
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.Invoking(s => s.BlockChatAsync(1, blockerId: 99))
                .Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task BlockChatAsync_ValidRequest_BlocksChatAndUpdatesRepository()
        {
            var chat = new Chat { User = new User { UserId = 1 } };
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.BlockChatAsync(1, blockerId: 1);

            chat.IsBlocked.Should().BeTrue();
            chat.BlockedByUser!.UserId.Should().Be(1);
            chatRepo.Received(1).UpdateAsync(chat, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UnblockChatAsync_ChatNotFound_ThrowsKeyNotFoundException()
        {
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Chat?)null);

            await chatService.Invoking(s => s.UnblockChatAsync(1, unblockerId: 1))
                .Should().ThrowAsync<KeyNotFoundException>();
        }


        [Fact]
        public async Task UnblockChatAsync_CallerIsNotBlocker_ThrowsUnauthorizedAccessException()
        {
            var chat = new Chat { User = new User { UserId = 1 }, SecondUser = new User { UserId = 2 }, BlockedByUser = new User { UserId = 2 } };
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.Invoking(s => s.UnblockChatAsync(1, unblockerId: 1))
                .Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task UnblockChatAsync_ValidRequest_UnblocksChatAndUpdatesRepository()
        {
            var chat = new Chat { User = new User { UserId = 1 }, BlockedByUser = new User { UserId = 1 }, IsBlocked = true };
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.UnblockChatAsync(1, unblockerId: 1);

            chat.IsBlocked.Should().BeFalse();
            chat.BlockedByUser.Should().BeNull();
            chatRepo.Received(1).UpdateAsync(chat, Arg.Any<CancellationToken>());
        }

        #endregion

        [Fact]
        public async Task DeleteChatAsync_ChatNotFound_ThrowsKeyNotFoundException()
        {
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Chat?)null);

            await chatService.Invoking(s => s.DeleteChatAsync(1, callerId: 1))
                .Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task DeleteChatAsync_CallerIsNotParticipant_ThrowsUnauthorizedAccessException()
        {
            var chat = new Chat { User = new User { UserId = 2 }, SecondUser = new User { UserId = 3 } };
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.Invoking(s => s.DeleteChatAsync(1, callerId: 99))
                .Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task DeleteChatAsync_CallerIsUser_SetsDeletedAtByUser()
        {
            var chat = new Chat { User = new User { UserId = 1 } };
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.DeleteChatAsync(1, callerId: 1);

            chat.DeletedAtByUser.Should().NotBeNull();
            chat.DeletedAtBySecondParty.Should().BeNull();
            chatRepo.Received(1).UpdateAsync(chat, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DeleteChatAsync_CallerIsSecondParty_SetsDeletedAtBySecondParty()
        {
            var chat = new Chat { User = new User { UserId = 1 }, SecondUser = new User { UserId = 2 } };
            chatRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(chat);

            await chatService.DeleteChatAsync(1, callerId: 2);

            chat.DeletedAtBySecondParty.Should().NotBeNull();
            chat.DeletedAtByUser.Should().BeNull();
            chatRepo.Received(1).UpdateAsync(chat, Arg.Any<CancellationToken>());
        }
    }
}