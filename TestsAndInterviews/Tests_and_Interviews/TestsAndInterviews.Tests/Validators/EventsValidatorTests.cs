using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests_and_Interviews.Validators;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace TestsAndInterviews.Tests.Validators
{
    [TestClass]
    public class EventsValidatorTests
    {
        [TestClass]
        public class EventValidatorTests
        {
            private const string ValidTitle = "Conference";
            private const string ErrorTitleMandatory = "Title is mandatory";
            private const string ErrorTitleTooLong = "Title is too long";

            private const string ValidDescription = "A very nice event.";
            private const string ErrorDescTooLong = "Description is too long";

            private const string ValidLocation = "Cluj-Napoca";
            private const string ErrorLocationMandatory = "Location is mandatory";
            private const string ErrorLocationTooLong = "Location is too long";

            private const string ErrorStartDateMandatory = "Starting date is mandatory";
            private const string ErrorStartDatePast = "Event must start after creation";

            private const string ErrorEndDateMandatory = "Ending date is mandatory";
            private const string ErrorEndDatePast = "Event must end after creation";

            private const string ErrorDatesChronological = "Event must begin before ending";

            private const char CharTitleFiller = 'A';
            private const char CharDescFiller = 'X';
            private const char CharLocFiller = 'L';

            private const int TitleMaxLength = 200;
            private const int TitleExceededLength = 201;

            private const int DescMaxLength = 2000;
            private const int DescExceededLength = 2001;

            private const int LocMaxLength = 300;
            private const int LocExceededLength = 301;

            private const int DaysPast = -1;
            private const int DaysFutureNear = 1;
            private const int DaysFutureMid = 2;
            private const int DaysFutureFar = 3;
            private const int DaysFutureVeryFar = 5;

            private EventValidator eventValidator = null!;

            [TestInitialize]
            public void Setup()
            {
                eventValidator = new EventValidator();
            }

            [TestMethod]
            public void IsEventTitleValid_NormalTitle_ReturnsTrue()
            {
                bool result = eventValidator.ValidateEventTitle(ValidTitle);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventTitleValid_EmptyTitle_ThrowsException()
            {
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.ValidateEventTitle(string.Empty));
                Assert.AreEqual(ErrorTitleMandatory, exception.Message);
            }

            [TestMethod]
            public void IsEventTitleValid_TitleExactly200Characters_ReturnsTrue()
            {
                string title = new string(CharTitleFiller, TitleMaxLength);
                bool result = eventValidator.ValidateEventTitle(title);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventTitleValid_Title201Characters_ThrowsException()
            {
                string title = new string(CharTitleFiller, TitleExceededLength);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.ValidateEventTitle(title));
                Assert.AreEqual(ErrorTitleTooLong, exception.Message);
            }

            [TestMethod]
            public void IsEventDescriptionValid_NormalDescription_ReturnsTrue()
            {
                bool result = eventValidator.ValidateEventDescription(ValidDescription);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventDescriptionValid_EmptyDescription_ReturnsTrue()
            {
                bool result = eventValidator.ValidateEventDescription(string.Empty);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventDescriptionValid_DescriptionExactly2000Characters_ReturnsTrue()
            {
                string description = new string(CharDescFiller, DescMaxLength);
                bool result = eventValidator.ValidateEventDescription(description);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventDescriptionValid_Description2001Characters_ThrowsException()
            {
                string description = new string(CharTitleFiller, DescExceededLength);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.ValidateEventDescription(description));
                Assert.AreEqual(ErrorDescTooLong, exception.Message);
            }

            [TestMethod]
            public void IsEventLocationValid_NormalLocation_ReturnsTrue()
            {
                bool result = eventValidator.ValidateEventLocation(ValidLocation);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventLocationValid_EmptyLocation_ThrowsException()
            {
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.ValidateEventLocation(string.Empty));
                Assert.AreEqual(ErrorLocationMandatory, exception.Message);
            }

            [TestMethod]
            public void IsEventLocationValid_LocationExactly300Characters_ReturnsTrue()
            {
                string location = new string(CharLocFiller, LocMaxLength);
                bool result = eventValidator.ValidateEventLocation(location);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventLocationValid_Location301Characters_ThrowsException()
            {
                string location = new string(CharLocFiller, LocExceededLength);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.ValidateEventLocation(location));
                Assert.AreEqual(ErrorLocationTooLong, exception.Message);
            }

            [TestMethod]
            public void IsEventStartDateValid_FutureDate_ReturnsTrue()
            {
                DateTimeOffset startDate = DateTimeOffset.Now.AddDays(DaysFutureNear);
                bool result = eventValidator.ValidateEventStartDate(startDate);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventStartDateValid_NullDate_ThrowsException()
            {
                DateTimeOffset? startDate = null;
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.ValidateEventStartDate(startDate));
                Assert.AreEqual(ErrorStartDateMandatory, exception.Message);
            }

            [TestMethod]
            public void IsEventStartDateValid_PastDate_ThrowsException()
            {
                DateTimeOffset startDate = DateTimeOffset.Now.AddDays(DaysPast);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.ValidateEventStartDate(startDate));
                Assert.AreEqual(ErrorStartDatePast, exception.Message);
            }

            [TestMethod]
            public void IsEventEndDateValid_FutureDate_ReturnsTrue()
            {
                DateTimeOffset endDate = DateTimeOffset.Now.AddDays(DaysFutureVeryFar);
                bool result = eventValidator.ValidateEventEndDate(endDate);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void IsEventEndDateValid_NullDate_ThrowsException()
            {
                DateTimeOffset? endDate = null;
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.ValidateEventEndDate(endDate));
                Assert.AreEqual(ErrorEndDateMandatory, exception.Message);
            }

            [TestMethod]
            public void IsEventEndDateValid_PastDate_ThrowsException()
            {
                DateTimeOffset endDate = DateTimeOffset.Now.AddDays(DaysPast);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.ValidateEventEndDate(endDate));
                Assert.AreEqual(ErrorEndDatePast, exception.Message);
            }

            [TestMethod]
            public void AreEventDatesCronologicallyValid_StartBeforeEnd_ReturnsTrue()
            {
                DateTimeOffset startDate = DateTimeOffset.Now.AddDays(DaysFutureNear);
                DateTimeOffset endDate = DateTimeOffset.Now.AddDays(DaysFutureFar);
                bool result = eventValidator.ValidateEventDatesChronologically(startDate, endDate);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void AreEventDatesCronologicallyValid_StartAfterEnd_ThrowsException()
            {
                DateTimeOffset startDate = DateTimeOffset.Now.AddDays(DaysFutureVeryFar);
                DateTimeOffset endDate = DateTimeOffset.Now.AddDays(DaysFutureNear);
                var exception = Assert.ThrowsException<Exception>(() =>
                    eventValidator.ValidateEventDatesChronologically(startDate, endDate));
                Assert.AreEqual(ErrorDatesChronological, exception.Message);
            }

            [TestMethod]
            public void AreEventDatesCronologicallyValid_StartEqualsEnd_ReturnsTrue()
            {
                DateTimeOffset date = DateTimeOffset.Now.AddDays(DaysFutureMid);
                bool result = eventValidator.ValidateEventDatesChronologically(date, date);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void AreEventDatesCronologicallyValid_NullStartDate_ReturnsTrue()
            {
                DateTimeOffset? startDate = null;
                DateTimeOffset endDate = DateTimeOffset.Now.AddDays(DaysFutureMid);
                bool result = eventValidator.ValidateEventDatesChronologically(startDate, endDate);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void AreEventDatesCronologicallyValid_NullEndDate_ReturnsTrue()
            {
                DateTimeOffset startDate = DateTimeOffset.Now.AddDays(DaysFutureNear);
                DateTimeOffset? endDate = null;
                bool result = eventValidator.ValidateEventDatesChronologically(startDate, endDate);
                Assert.IsTrue(result);
            }
        }
    }
}