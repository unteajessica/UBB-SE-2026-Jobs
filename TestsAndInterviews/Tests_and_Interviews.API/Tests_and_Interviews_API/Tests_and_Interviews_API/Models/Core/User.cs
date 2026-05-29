namespace Tests_and_Interviews_API.Models.Core
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Users")]
    public class User
    {
        public User(int id, string firstName, string lastName, string email, string passwordHash, string role = "Candidate", string? cvXml = null)
        {
            this.Id = id;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Email = email;
            this.PasswordHash = passwordHash;
            this.Role = role;
            this.CvXml = cvXml;
        }

        protected User() { }

        [Key]
        [Column("UserId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("FirstName", TypeName = "nvarchar(100)")]
        public string FirstName { get; set; } = string.Empty;

        [Column("LastName", TypeName = "nvarchar(100)")]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string Name => $"{FirstName} {LastName}".Trim();

        [Column("Email", TypeName = "nvarchar(256)")]
        public string Email { get; set; } = string.Empty;

        [Column("ParsedCv", TypeName = "nvarchar(max)")]
        public string? CvXml { get; set; }

        [Column("PasswordHash", TypeName = "nvarchar(512)")]
        public string PasswordHash { get; set; } = string.Empty;

        [NotMapped]
        public string Role { get; set; } = "Candidate";
    }
}
