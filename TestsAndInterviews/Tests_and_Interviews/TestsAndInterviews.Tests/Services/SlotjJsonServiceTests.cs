namespace TestsAndInterviews.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Services;

    public class SlotJsonServiceTests
    {
        private readonly string _actualFilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "slots.json");
        public SlotJsonServiceTests()
        {
            if (File.Exists(_actualFilePath))
            {
                File.Delete(_actualFilePath);
            }
        }

        [Fact]
        public void LoadSlots_FileDoesNotExist_ReturnsEmptyList()
        {
            var result = SlotJsonService.LoadSlots();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void SaveAndLoad_ValidData_MaintainsIntegrity()
        {
            var slots = new List<Slot> { new Slot { Id = 1, InterviewType = "Technical" } };

            SlotJsonService.SaveSlots(slots);
            var result = SlotJsonService.LoadSlots();
            Assert.Single(result);
            Assert.Equal("Technical", result[0].InterviewType);
        }
        [Fact]
        public void SaveSlots_ExistingFile_OverwritesSuccessfully()
        {
            // Arrange
            var initial = new List<Slot> { new Slot { Id = 1 } };
            var updated = new List<Slot> { new Slot { Id = 2 } };

            // Act
            SlotJsonService.SaveSlots(initial);
            SlotJsonService.SaveSlots(updated);
            var result = SlotJsonService.LoadSlots();

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result[0].Id);
        }

       
    }
}