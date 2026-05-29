using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests_and_Interviews.Validators;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace TestsAndInterviews.Tests.Validators
{
    [TestClass]
    public class CompanyValidatorTests
    {
        private const char FillerChar = 'a';
        private const int NameLengthExceeded = 201;
        private const int AboutLengthExceeded = 2001;
        private const int LocationLengthExceeded = 301;
        private const int EmailLengthExceeded = 101;

        private const string ValidName = "Google";
        private const string ValidAboutUs = "We build software.";
        private const string ValidLocation = "Bucharest";
        private const string InvalidEmailNoAt = "invalidemail";
        private const string EmailDomain = "@gmail.com";
        private const string ValidEmail = "test@gmail.com";
        private const string InvalidImageExt = "image.txt";
        private const string ValidImagePng = "image.png";
        private const string InvalidLogoExt = "logo.txt";
        private const string ValidLogoJpg = "logo.jpg";
        private const string ValidBase64Png = "data:image/png;base64,AAA";
        private const string ValidBase64Jpeg = "data:image/jpeg;base64,AAA";
        private const string WhitespaceString = "   ";

        private CompanyValidator validator = null!;

        [TestInitialize]
        public void Setup()
        {
            validator = new CompanyValidator();
        }

        [TestMethod]
        public void NameValidator_ValidName_ReturnsTrue()
        {
            bool result = validator.ValidateName(ValidName);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void NameValidator_EmptyName_ThrowsException()
        {
            Action action = () => validator.ValidateName(string.Empty);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void NameValidator_NameTooLong_ThrowsException()
        {
            string name = new string(FillerChar, NameLengthExceeded);
            Action action = () => validator.ValidateName(name);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void AboutUsValidator_EmptyAboutUs_ReturnsTrue()
        {
            bool result = validator.ValidateAboutUs(string.Empty);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AboutUsValidator_TooLongAboutUs_ThrowsException()
        {
            string about = new string(FillerChar, AboutLengthExceeded);
            Action action = () => validator.ValidateAboutUs(about);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void AboutUsValidator_NullAboutUs_ReturnsTrue()
        {
            bool result = validator.ValidateAboutUs(null!);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AboutUsValidator_ValidAboutUs_ReturnsTrue()
        {
            bool result = validator.ValidateAboutUs(ValidAboutUs);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LocationValidator_EmptyLocation_ReturnsTrue()
        {
            bool result = validator.ValidateLocation(string.Empty);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LocationValidator_LocationTooLong_ThrowsException()
        {
            string location = new string(FillerChar, LocationLengthExceeded);
            Action action = () => validator.ValidateLocation(location);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void LocationValidator_NullLocation_ReturnsTrue()
        {
            bool result = validator.ValidateLocation(null!);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LocationValidator_ValidLocation_ReturnsTrue()
        {
            bool result = validator.ValidateLocation(ValidLocation);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EmailValidator_EmptyEmail_ReturnsTrue()
        {
            bool result = validator.ValidateEmail(string.Empty);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EmailValidator_EmailWithoutAt_ThrowsException()
        {
            Action action = () => validator.ValidateEmail(InvalidEmailNoAt);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void EmailValidator_EmailTooLong_ThrowsException()
        {
            string email = new string(FillerChar, EmailLengthExceeded) + EmailDomain;
            Action action = () => validator.ValidateEmail(email);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void EmailValidator_ValidEmail_ReturnsTrue()
        {
            bool result = validator.ValidateEmail(ValidEmail);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PfpValidator_EmptyPfp_ReturnsTrue()
        {
            bool result = validator.ValidateProfilePicture(string.Empty);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PfpValidator_InvalidExtension_ThrowsException()
        {
            Action action = () => validator.ValidateProfilePicture(InvalidImageExt);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void PfpValidator_ValidImage_ReturnsTrue()
        {
            bool result = validator.ValidateProfilePicture(ValidImagePng);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LogoValidator_EmptyLogo_ThrowsException()
        {
            Action action = () => validator.ValidateLogo(string.Empty);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void LogoValidator_InvalidExtension_ThrowsException()
        {
            Action action = () => validator.ValidateLogo(InvalidLogoExt);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void LogoValidator_ValidLogo_ReturnsTrue()
        {
            bool result = validator.ValidateLogo(ValidLogoJpg);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PfpValidator_DataImagePng_ReturnsTrue()
        {
            bool result = validator.ValidateProfilePicture(ValidBase64Png);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LogoValidator_DataImageJpeg_ReturnsTrue()
        {
            bool result = validator.ValidateLogo(ValidBase64Jpeg);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PfpValidator_WhitespaceImage_ThrowsException()
        {
            Action action = () => validator.ValidateProfilePicture(WhitespaceString);
            Assert.ThrowsException<Exception>(action);
        }
    }
}