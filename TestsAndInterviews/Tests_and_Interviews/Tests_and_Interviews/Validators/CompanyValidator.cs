using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_and_Interviews.Validators
{
    public class CompanyValidator : ICompanyValidator
    {
        private const int MaximumNameLength = 200;
        private const int MaximumAboutUsLength = 2000;
        private const int MaximumLocationLength = 300;
        private const int MaximumEmailLength = 100;
        private const string RequiredEmailCharacter = "@";
        private const string Base64ImagePrefix = "data:image/";

        private const string ExtensionJpg = ".jpg";
        private const string ExtensionJpeg = ".jpeg";
        private const string ExtensionPng = ".png";
        private const string MimeTypeJpg = "jpg";
        private const string MimeTypeJpeg = "jpeg";
        private const string MimeTypePng = "png";

        public bool ValidateName(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                throw new Exception("Name is mandatory");
            }
            if (companyName.Length > MaximumNameLength)
            {
                throw new Exception("Name is too long");
            }
            return true;
        }

        public bool ValidateAboutUs(string aboutUsDescription)
        {
            if (string.IsNullOrEmpty(aboutUsDescription))
            {
                return true;
            }
            if (aboutUsDescription.Length > MaximumAboutUsLength)
            {
                throw new Exception("AboutUs is too long");
            }
            return true;
        }

        public bool ValidateLocation(string companyLocation)
        {
            if (string.IsNullOrEmpty(companyLocation))
            {
                return true;
            }
            if (companyLocation.Length > MaximumLocationLength)
            {
                throw new Exception("Location is too long");
            }
            return true;
        }

        public bool ValidateEmail(string companyEmail)
        {
            if (string.IsNullOrEmpty(companyEmail))
            {
                return true;
            }
            if (!companyEmail.Contains(RequiredEmailCharacter))
            {
                throw new Exception("Email must contain '@'");
            }
            if (companyEmail.Length > MaximumEmailLength)
            {
                throw new Exception("Email is too long");
            }
            return true;
        }

        public bool ValidateProfilePicture(string profilePicturePath)
        {
            if (string.IsNullOrEmpty(profilePicturePath))
            {
                return true;
            }
            if (!HasAllowedImageExtension(profilePicturePath))
            {
                throw new Exception("Profile picture must be .jpg, .jpeg or .png");
            }
            return true;
        }

        public bool ValidateLogo(string logoPath)
        {
            if (string.IsNullOrWhiteSpace(logoPath))
            {
                throw new Exception("Logo is mandatory");
            }
            if (!HasAllowedImageExtension(logoPath))
            {
                throw new Exception("Logo must be .jpg, .jpeg or .png");
            }
            return true;
        }

        private static bool HasAllowedImageExtension(string imagePathOrData)
        {
            if (string.IsNullOrWhiteSpace(imagePathOrData))
            {
                return false;
            }

            if (imagePathOrData.EndsWith(ExtensionJpg, StringComparison.OrdinalIgnoreCase)
                || imagePathOrData.EndsWith(ExtensionJpeg, StringComparison.OrdinalIgnoreCase)
                || imagePathOrData.EndsWith(ExtensionPng, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (imagePathOrData.StartsWith(Base64ImagePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var remainingString = imagePathOrData.Substring(Base64ImagePrefix.Length);
                var semicolonIndex = remainingString.IndexOf(';');
                var mimeSubtype = (semicolonIndex >= 0 ? remainingString.Substring(0, semicolonIndex) : remainingString)
                    .Trim()
                    .ToLowerInvariant();

                return mimeSubtype == MimeTypePng
                    || mimeSubtype == MimeTypeJpeg
                    || mimeSubtype == MimeTypeJpg;
            }

            return false;
        }
    }
}