namespace Tests_and_Interviews.Validators
{
    public interface ICompanyValidator
    {
        bool ValidateName(string companyName);
        bool ValidateAboutUs(string aboutUsDescription);
        bool ValidateLocation(string companyLocation);
        bool ValidateEmail(string companyEmail);
        bool ValidateProfilePicture(string profilePicturePath);
        bool ValidateLogo(string logoPath);
    }
}