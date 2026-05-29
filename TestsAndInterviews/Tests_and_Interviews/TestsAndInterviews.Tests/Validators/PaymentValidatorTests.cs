using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests_and_Interviews.Validators;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace TestsAndInterviews.Tests.Validators
{
    [TestClass]
    public class PaymentValidatorTests
    {
        private const string ValidName = "John Doe";
        private const string WhitespaceInput = "   ";

        private const string ValidCard = "123456789012345";
        private const string ShortCard = "123";

        private const string ValidExp = "12/99";
        private const string ExpNoSlash = "1299";
        private const string ExpInvalidChars = "ab/cd";
        private const string ExpMissingYear = "12/";
        private const string ExpMonthLow = "00/99";
        private const string ExpMonthHigh = "13/99";

        private const string ValidCvv = "123";
        private const string ShortCvvValue = "12";

        private const string ErrNameRequired = "Card Holder Name is required.";
        private const string ErrCardInvalid = "Please enter a valid Card Number.";
        private const string ErrExpFormat = "Expiration Date must be in MM/YY format.";
        private const string ErrExpNumbers = "Expiration Date must contain valid numbers (MM/YY).";
        private const string ErrExpMonthRange = "Invalid expiration month. Must be between 01 and 12.";
        private const string ErrCvvInvalid = "Please enter a valid CVV.";
        private const string ErrExpired = "This card has expired. Please use a valid card.";

        private const string FormatTwoDigits = "00";
        private const string SlashSeparator = "/";
        private const int MonthJanuary = 1;
        private const int MonthDecember = 12;
        private const int OffsetOne = 1;
        private const int YearBase2000 = 2000;

        private PaymentValidator validator = null!;

        [TestInitialize]
        public void Setup()
        {
            validator = new PaymentValidator();
        }

        [TestMethod]
        public void ValidatePaymentDetails_EmptyName_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(string.Empty, ValidCard, ValidExp, ValidCvv);

            Assert.AreEqual(ErrNameRequired, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_WhitespaceName_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(WhitespaceInput, ValidCard, ValidExp, ValidCvv);

            Assert.AreEqual(ErrNameRequired, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_EmptyCardNumber_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, string.Empty, ValidExp, ValidCvv);

            Assert.AreEqual(ErrCardInvalid, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_WhitespaceCardNumber_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, WhitespaceInput, ValidExp, ValidCvv);

            Assert.AreEqual(ErrCardInvalid, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_ShortCardNumber_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ShortCard, ValidExp, ValidCvv);

            Assert.AreEqual(ErrCardInvalid, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_EmptyExpirationDate_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, string.Empty, ValidCvv);

            Assert.AreEqual(ErrExpFormat, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_WhitespaceExpirationDate_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, WhitespaceInput, ValidCvv);

            Assert.AreEqual(ErrExpFormat, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_ExpirationWithoutSlash_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, ExpNoSlash, ValidCvv);

            Assert.AreEqual(ErrExpFormat, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_ExpirationWithInvalidNumbers_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, ExpInvalidChars, ValidCvv);

            Assert.AreEqual(ErrExpNumbers, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_ExpirationWithMissingYearPart_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, ExpMissingYear, ValidCvv);

            Assert.AreEqual(ErrExpNumbers, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_MonthBelowRange_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, ExpMonthLow, ValidCvv);

            Assert.AreEqual(ErrExpMonthRange, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_MonthAboveRange_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, ExpMonthHigh, ValidCvv);

            Assert.AreEqual(ErrExpMonthRange, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_EmptyCvv_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, ValidExp, string.Empty);

            Assert.AreEqual(ErrCvvInvalid, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_WhitespaceCvv_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, ValidExp, WhitespaceInput);

            Assert.AreEqual(ErrCvvInvalid, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_ShortCvv_ReturnsError()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, ValidExp, ShortCvvValue);

            Assert.AreEqual(ErrCvvInvalid, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_PreviousMonthInCurrentYear_ReturnsExpiredError()
        {
            var today = DateTime.Now;
            var previousMonth = today.Month == MonthJanuary ? MonthDecember : today.Month - OffsetOne;
            var year = today.Month == MonthJanuary ? today.Year - OffsetOne : today.Year;
            var yy = (year - YearBase2000).ToString(FormatTwoDigits);
            var exp = $"{previousMonth.ToString(FormatTwoDigits)}{SlashSeparator}{yy}";

            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, exp, ValidCvv);

            Assert.AreEqual(ErrExpired, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_PreviousYear_ReturnsExpiredError()
        {
            var yy = ((DateTime.Now.Year - OffsetOne) - YearBase2000).ToString(FormatTwoDigits);
            var exp = $"{MonthDecember.ToString(FormatTwoDigits)}{SlashSeparator}{yy}";

            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, exp, ValidCvv);

            Assert.AreEqual(ErrExpired, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_CurrentMonthAndYear_ReturnsEmptyString()
        {
            var today = DateTime.Now;
            var exp = $"{today.Month.ToString(FormatTwoDigits)}{SlashSeparator}{(today.Year - YearBase2000).ToString(FormatTwoDigits)}";

            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, exp, ValidCvv);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ValidatePaymentDetails_FutureDate_ReturnsEmptyString()
        {
            var result = validator.ValidatePaymentDetails(ValidName, ValidCard, ValidExp, ValidCvv);

            Assert.AreEqual(string.Empty, result);
        }
    }
}