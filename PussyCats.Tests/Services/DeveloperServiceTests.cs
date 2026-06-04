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
            Assert.Contains("Choose a valid parameter.", exception.Message);
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
            Assert.Contains("Value cannot be empty.", exception.Message);
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

            Assert.Equal(developer.DeveloperId, post.Developer.DeveloperId);
            Assert.Equal(postMessage, post.Value);
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
            Assert.Contains(interactions, interaction => interaction.Developer.DeveloperId == developer.DeveloperId);
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
            Assert.Contains(interactions, interaction => interaction.Type == DeveloperInteractionType.Dislike);
        }
    }
}
