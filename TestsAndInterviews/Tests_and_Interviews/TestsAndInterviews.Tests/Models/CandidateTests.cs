// <copyright file="CandidateTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TestsAndInterviews.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using Tests_and_Interviews.Models;
    using Tests_and_Interviews.Models.Enums;
    using Xunit;

    public class CandidateTests
    {
        [Fact]
        public void ViewAvailableSlots_WhenSlotsExistForDate_ReturnsMatchingSlots()
        {
            var date = DateTime.Today;
            var candidate = new Candidate
            {
                AvailableSlots = new List<Slot>
                {
                    new Slot { StartTime = date },
                    new Slot { StartTime = date.AddDays(1) },
                },
            };

            var result = candidate.ViewAvailableSlots(date);

            Assert.Single(result);
            Assert.Equal(date.Date, result[0].StartTime.Date);
        }

        [Fact]
        public void ViewAvailableSlots_WhenNoSlotsExistForDate_ReturnsEmpty()
        {
            var candidate = new Candidate
            {
                AvailableSlots = new List<Slot>
                {
                    new Slot { StartTime = DateTime.Today.AddDays(1) },
                },
            };

            var result = candidate.ViewAvailableSlots(DateTime.Today);

            Assert.Empty(result);
        }

        [Fact]
        public void BookSlot_WhenSlotIsNotAvailable_ThrowsException()
        {
            var candidate = new Candidate { Id = 1 };
            var slot = new Slot { Status = SlotStatus.Occupied };
            bool caught = false;
            try
            {
                candidate.BookSlot(slot);
            }
            catch (Exception)
            {
                caught = true;
            }
            Assert.True(caught);
        }
    }
}