namespace Tests_and_Interviews.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Company class represents a company entity in the system, containing properties such as company ID, name, about us section, profile picture URL, logo URL, location, email, posted jobs count, collaborators count, and various scenario-related properties for the company's buddy character.
    /// </summary>
    [Table("companies")]
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("company_id")]
        public int CompanyId { get; set; }

        [Column("company_name", TypeName = "nvarchar(255)")]
        public string Name { get; set; } = string.Empty;

        [Column("about_us", TypeName = "nvarchar(max)")]
        public string? AboutUs { get; set; }

        [Column("profile_picture_url", TypeName = "nvarchar(max)")]
        public string? ProfilePicturePath { get; set; }

        [Column("logo_picture_url", TypeName = "nvarchar(max)")]
        public string CompanyLogoPath { get; set; } = string.Empty;

        [Column("location", TypeName = "nvarchar(300)")]
        public string? Location { get; set; }

        [Column("email", TypeName = "nvarchar(100)")]
        public string? Email { get; set; }

        [Column("posted_jobs_count")]
        public int PostedJobsCount { get; set; }

        [Column("collaborators_count")]
        public int CollaboratorsCount { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Company"/> class with default values.
        /// </summary>
        public Company() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Company"/> class with specified values for the company's properties, including name, about us section, profile picture URL, logo URL, location, email, company ID, posted jobs count, and collaborators count.
        /// </summary>
        /// <param name="name">The name of the company.</param>
        /// <param name="aboutUs">The about us section of the company.</param>
        /// <param name="pfpUrl">The profile picture URL of the company.</param>
        /// <param name="logoUrl">The logo URL of the company.</param>
        /// <param name="location">The location of the company.</param>
        /// <param name="email">The email address of the company.</param>
        /// <param name="companyId">The unique identifier of the company.</param>
        /// <param name="postedJobsCount">The number of jobs posted by the company.</param>
        /// <param name="collaboratorsCount">The number of collaborators associated with the company.</param>
        public Company(
            string name,
            string aboutUs,
            string pfpUrl,
            string logoUrl,
            string location,
            string email,
            int companyId = 1,
            int postedJobsCount = 0,
            int collaboratorsCount = 0)
        {
            this.CompanyId = companyId;
            this.Name = name ?? string.Empty;
            this.AboutUs = aboutUs ?? string.Empty;
            this.ProfilePicturePath = pfpUrl ?? string.Empty;
            this.CompanyLogoPath = logoUrl ?? string.Empty;
            this.Location = location ?? string.Empty;
            this.Email = email ?? string.Empty;
            this.PostedJobsCount = postedJobsCount;
            this.CollaboratorsCount = collaboratorsCount;
        }

        private Game? game;
        /// <summary>
        /// Gets or sets the Game associated with the company. This property is marked with the [NotMapped] attribute, indicating that it does not correspond to a column in the database and is used solely for navigation purposes within the application.
        /// </summary>
        [NotMapped]
        public Game? Game
        {
            get => this.game;
            set => this.game = value;
        }

        /// <summary>
        /// Gets or sets the name of the company's buddy character.
        /// </summary>
        [Column("buddy_name", TypeName = "nvarchar(255)")]
        public string? BuddyName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the avatar associated with the company's buddy character.
        /// </summary>
        [Column("avatar_id")]
        public int? AvatarId { get; set; }

        /// <summary>
        /// Gets or sets the final quote associated with the company's buddy character.
        /// </summary>
        [Column("final_quote", TypeName = "nvarchar(max)")]
        public string? FinalQuote { get; set; }

        /// <summary>
        /// Gets or sets the description of the company's buddy character.
        /// </summary>
        [Column("buddy_description", TypeName = "nvarchar(255)")]
        public string? BuddyDescription { get; set; }

        /// <summary>
        /// Gets or sets the text for the first scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen_1_text", TypeName = "nvarchar(max)")]
        public string? Scen1Text { get; set; }

        /// <summary>
        /// Gets or sets the first answer option for the first scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen1_answer1", TypeName = "nvarchar(max)")]
        public string? Scen1Answer1 { get; set; }

        /// <summary>
        /// Gets or sets the second answer option for the first scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen1_answer2", TypeName = "nvarchar(max)")]
        public string? Scen1Answer2 { get; set; }

        /// <summary>
        /// Gets or sets the third answer option for the first scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen1_answer3", TypeName = "nvarchar(max)")]
        public string? Scen1Answer3 { get; set; }

        /// <summary>
        /// Gets or sets the first reaction option for the first scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen1_reaction1", TypeName = "nvarchar(max)")]
        public string? Scen1Reaction1 { get; set; }

        /// <summary>
        /// Gets or sets the second reaction option for the first scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen1_reaction2", TypeName = "nvarchar(max)")]
        public string? Scen1Reaction2 { get; set; }

        /// <summary>
        /// Gets or sets the third reaction option for the first scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen1_reaction3", TypeName = "nvarchar(max)")]
        public string? Scen1Reaction3 { get; set; }

        /// <summary>
        /// Gets or sets the text for the second scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen2_text", TypeName = "nvarchar(max)")]
        public string? Scen2Text { get; set; }

        /// <summary>
        /// Gets or sets the first answer option for the second scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen2_answer1", TypeName = "nvarchar(max)")]
        public string? Scen2Answer1 { get; set; }

        /// <summary>
        /// Gets or sets the second answer option for the second scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen2_answer2", TypeName = "nvarchar(max)")]
        public string? Scen2Answer2 { get; set; }

        /// <summary>
        /// Gets or sets the third answer option for the second scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen2_answer3", TypeName = "nvarchar(max)")]
        public string? Scen2Answer3 { get; set; }

        /// <summary>
        /// Gets or sets the first reaction option for the second scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen2_reaction1", TypeName = "nvarchar(max)")]
        public string? Scen2Reaction1 { get; set; }

        /// <summary>
        /// Gets or sets the second reaction option for the second scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen2_reaction2", TypeName = "nvarchar(max)")]
        public string? Scen2Reaction2 { get; set; }

        /// <summary>
        /// Gets or sets the third reaction option for the second scenario associated with the company's buddy character.
        /// </summary>
        [Column("scen2_reaction3", TypeName = "nvarchar(max)")]
        public string? Scen2Reaction3 { get; set; }

        /// <summary>
        /// Gets or sets the collection of job postings associated with the company.
        /// </summary>
        public ICollection<JobPosting> Jobs { get; set; } = new List<JobPosting>();

        /// <summary>
        /// Gets or sets the collection of events associated with the company.
        /// </summary>
        public ICollection<Event> Events { get; set; } = new List<Event>();

        /// <summary>
        /// Gets or sets the collection of collaborators associated with the company.
        /// </summary>
        public ICollection<Collaborator> Collaborators { get; set; } = new List<Collaborator>();

        /// <summary>
        /// Returns a string representation of the Company object, including the company ID, name, and email address.
        /// </summary>
        /// <returns>A string representation of the Company object.</returns>
        public override string ToString()
        {
            return $"Company[{this.CompanyId}]: {this.Name}, {this.Email}";
        }
    }
}