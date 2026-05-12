using FluentAssertions;
using NSubstitute;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
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

    }
}