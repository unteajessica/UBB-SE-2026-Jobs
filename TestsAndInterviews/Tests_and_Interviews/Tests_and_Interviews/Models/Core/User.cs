namespace Tests_and_Interviews.Models.Core
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents an User.
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="id">The user's unique identifier.</param>
        /// <param name="name">The user's name.</param>
        /// <param name="email">The user's email.</param>
        /// <param name="cvXml">The user's cv in xml format.</param>
        public User(int id, string name, string email, string? cvXml = null)
        {
            this.Id = id;
            this.Name = name;
            this.Email = email;
            this.CvXml = cvXml;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// Parameterless constructor used by EF Core, hence the protected status.
        /// </summary>
        protected User()
        {
        }

        /// <summary>
        /// Gets or sets the unique identifier for the User.
        /// </summary>
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        [Column("name",TypeName = "nvarchar(255)")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user's email.
        /// </summary>
        [Column("email", TypeName = "nvarchar(255)")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's cv in xml format.
        /// </summary>
        [Column("cv_xml", TypeName = "nvarchar(max)")]
        public string? CvXml { get; set; }

    }
}