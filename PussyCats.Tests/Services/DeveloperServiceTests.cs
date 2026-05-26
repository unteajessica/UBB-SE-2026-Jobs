using FluentAssertions;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using PussyCats.Library.Services.Developers;

namespace PussyCats.Tests.Services
{
    public class DeveloperServiceTests
    {
        private readonly DeveloperService developerService = new DeveloperService();

        [Fact]
        public async Task AddPost_UnknownPostParameterType_ThrowsArgumentException()
        {
            Developer developer = new Developer
            {
                DeveloperId = 1,
                Name = "John"
            };
            string postMessage = "Hello world!";
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => developerService.AddPostAsync(developer.DeveloperId, DeveloperPostParameterType.Unknown, postMessage));
            exception.Message.Should().Contain("Choose a valid parameter.");
        }

        [Fact]
        public async Task AddPost_EmptyMessage_ThrowsArgumentException()
        {
            Developer developer = new Developer
            {
                DeveloperId = 1,
                Name = "John"
            };
            string postMessage = string.Empty;
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => developerService.AddPostAsync(developer.DeveloperId, DeveloperPostParameterType.WeightedDistanceScoreWeight, postMessage));
            exception.Message.Should().Contain("Value cannot be empty.");
        }

        [Fact]
        public async Task AddPost_ValidInput_AddsPostSuccessfully()
        {
            Developer developer = new Developer
            {
                DeveloperId = 1,
                Name = "John"
            };
            string postMessage = "Hello world!";
            var post = await developerService.AddPostAsync(developer.DeveloperId, DeveloperPostParameterType.WeightedDistanceScoreWeight, postMessage);

            post.Developer.DeveloperId.Should().Be(developer.DeveloperId);
            post.Value.Should().Be(postMessage);
        }

        [Fact]
        public async Task AddInteraction_NoPreviousInteraction_AddsInteraction()
        {
            Developer developer = new Developer
            {
                DeveloperId = 1,
                Name = "John"
            };
            var post = await developerService.AddPostAsync(developer.DeveloperId, DeveloperPostParameterType.WeightedDistanceScoreWeight, "Hello world!");

            await developerService.AddInteractionAsync(developer.DeveloperId, post.DeveloperPostId, DeveloperInteractionType.Like);
            var interactions = await developerService.GetInteractionsAsync();
            interactions.Should().Contain(interaction => interaction.Developer.DeveloperId == developer.DeveloperId);
        }

        [Fact]
        public async Task AddInteraction_ExistingInteraction_UpdatesInteractionType()
        {
            Developer developer = new Developer
            {
                DeveloperId = 1,
                Name = "John"
            };
            var post = await developerService.AddPostAsync(developer.DeveloperId, DeveloperPostParameterType.WeightedDistanceScoreWeight, "Hello world!");

            await developerService.AddInteractionAsync(developer.DeveloperId, post.DeveloperPostId, DeveloperInteractionType.Like);
            await developerService.AddInteractionAsync(developer.DeveloperId, post.DeveloperPostId, DeveloperInteractionType.Dislike);

            var interactions = await developerService.GetInteractionsAsync();
            interactions.Should().Contain(interaction => interaction.Type == DeveloperInteractionType.Dislike);
        }
    }
}
