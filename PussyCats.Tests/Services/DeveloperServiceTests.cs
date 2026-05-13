using FluentAssertions;
using NSubstitute.ExceptionExtensions;
using PussyCats.App.Services;
using PussyCats.Library.Domain;
using PussyCats.Library.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PussyCats.Tests.Services
{
    public class DeveloperServiceTests
    {
        private DeveloperService developerService = new DeveloperService();

        [Fact]
        public void AddPost_UnknownPostParameterType_ThrowsArgumentException()
        {
            Developer developer = new Developer
            {
                DeveloperId = 1,
                Name = "John"
            };
            string postMessage = "Hello world!";
            var exception = Assert.Throws<ArgumentException>(() => developerService.AddPost(developer.DeveloperId, DeveloperPostParameterType.Unknown, postMessage));
            exception.Message.Should().Contain("Choose a valid parameter.");
        }

        [Fact]
        public void AddPost_EmptyMessage_ThrowsArgumentException()
        {
            Developer developer = new Developer
            {
                DeveloperId = 1,
                Name = "John"
            };
            string postMessage = string.Empty;
            var exception = Assert.Throws<ArgumentException>(() => developerService.AddPost(developer.DeveloperId, DeveloperPostParameterType.WeightedDistanceScoreWeight, postMessage));
            exception.Message.Should().Contain("Value cannot be empty.");
        }

        [Fact]
        public void AddPost_ValidInput_AddsPostSuccessfully()
        {
            Developer developer = new Developer
            {
                DeveloperId = 1,
                Name = "John"
            };
            string postMessage = "Hello world!";
            var post = developerService.AddPost(developer.DeveloperId, DeveloperPostParameterType.WeightedDistanceScoreWeight, postMessage);

            post.Developer.DeveloperId.Should().Be(developer.DeveloperId);
            post.Value.Should().Be(postMessage);
        }
    }
}
