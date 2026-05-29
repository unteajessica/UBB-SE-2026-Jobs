namespace PussyCats.Web.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string? CvXml { get; set; }

        public bool HasCv => !string.IsNullOrWhiteSpace(this.CvXml);
    }
}
